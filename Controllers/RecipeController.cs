using AltRecipe;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace RecipeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly DietaryDictionary _dictionary;
    private readonly HttpClient _httpClient;

    // Dependency injection handles bringing in your dictionary and client setup
    public RecipeController(DietaryDictionary dictionary, HttpClient httpClient)
    {
        _dictionary = dictionary;
        _httpClient = httpClient;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessRecipe([FromBody] RecipeRequest request)
    {
        // Fail fast if payload is invalid
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Recipe text cannot be empty.");
        }

        // 1. Local RAG Lookup: Scan text against dictionary rules
        var substitutions = _dictionary.GetContextFor(request.Text, request.Constraint).ToList();

        // 2. Base Prompt Assembly
        var systemPrompt = $@"You are a culinary assistant. Extract the recipe into a JSON object with 'ingredients' (array of strings) and 'steps' (array of strings).Scale all ingredient quantities by a factor of {request.Scale}.";

        // 3. Conditional Augmentation: Only inject rules if matches were found
        if (substitutions.Any())
        {
            var contextBlock = string.Join(" ", substitutions);
            systemPrompt += $"\nCRITICAL CONTEXT: Apply these substitutions before outputting the ingredients: {contextBlock}";
        }

        systemPrompt += $"\n\nRecipe:\n{request.Text}";

        // 4. Formulate the Ollama payload
        var ollamaPayload = new
        {
            model = "gemma:2b",
            prompt = systemPrompt,
            stream = false,
            format = "json" // Forces local model to adhere to valid JSON schema
        };

        try
        {
            // 5. Dispatch to local Inference Layer
            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", ollamaPayload);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error communicating with the local LLM engine.");
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

            // Return the raw JSON string directly to the Angular UI
            return Ok(result?.Response);
        }
        catch (HttpRequestException ex)
        {
            // Gracefully catch cases where Ollama runner is turned off in the background
            return StatusCode(503, $"Ollama background instance is unreachable: {ex.Message}");
        }
    }
}