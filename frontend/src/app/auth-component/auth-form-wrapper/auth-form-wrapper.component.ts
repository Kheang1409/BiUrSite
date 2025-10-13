import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthIntroComponent } from '../auth-intro/auth-intro.component';

@Component({
  selector: 'app-auth-form-wrapper',
  standalone: true, // <-- important
  imports: [CommonModule, AuthIntroComponent],
  templateUrl: './auth-form-wrapper.component.html',
  styleUrls: ['./auth-form-wrapper.component.css'],
})
export class AuthFormWrapperComponent {}
