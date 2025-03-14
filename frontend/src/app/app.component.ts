import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { HeaderComponent } from './header/header.component';
import { CommonModule } from '@angular/common';
import { ToTopComponent } from './to-top/to-top.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, ToTopComponent, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  constructor(private _router: Router) {}

  isLoginPage(): boolean {
    return this._router.url === '/login';
  }

  isRegisterPage(): boolean {
    return this._router.url === '/register';
  }

  isForgotPasswordPage(): boolean{
    return this._router.url === '/forgot-password';
  }

  isResetPasswordPage(): boolean{
    return this._router.url === '/reset-password';
  }

  isConfirmationPage(): boolean {
    return this._router.url === '/confirmation-required';
  }

  isProfilePage(): boolean{
    return this._router.url === '/profile';
  }
}
