import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  recipe = '';

  diet = '';

  dietOptions = [
    'Vegetarian',
    'Vegan',
    'Dairy Free'
  ];

  onProceed() {
    console.log(this.recipe);
    console.log(this.diet);
  }
}
