import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-error-page',
  imports: [],
  templateUrl: './error-page.component.html',
  styleUrl: './error-page.component.css'
})
export class ErrorPageComponent {
  @Input() errorCode: string = '404'; // Default error code
  @Input() errorMessage: string = 'Page Not Found'; // Default error message
}
