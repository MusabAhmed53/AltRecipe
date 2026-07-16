namespace AltRecipe
{
    public class DietaryDictionary
    {
        // A simplified structure for concept mapping
        private readonly Dictionary<string, Dictionary<string, string>> _substitutions = new()
        {
            {
                "Dairy-Free", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "heavy cream", "coconut cream" },
                    { "butter", "vegan butter" },
                    { "milk", "oat milk" }
                }
            }
        };

        public IEnumerable<string> GetContextFor(string recipeText, string constraint)
        {
            if (!_substitutions.TryGetValue(constraint, out var rules)) yield break;

            foreach (var rule in rules)
            {
                if (recipeText.Contains(rule.Key, StringComparison.OrdinalIgnoreCase))
                {
                    yield return $"Replace '{rule.Key}' with '{rule.Value}'.";
                }
            }
        }
    }

    public record RecipeRequest(
        string Text,
        string Constraint,
        double Scale
    );

    public class OllamaResponse
    {
        // Ollama wraps the target model response inside a top-level "response" text property
        public string Response { get; set; } = string.Empty;
    }
}
