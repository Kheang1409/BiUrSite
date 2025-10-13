import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AuthFormWrapperComponent } from '../auth-form-wrapper/auth-form-wrapper.component';

@Component({
  selector: 'app-forgot-password',
  imports: [FormsModule, CommonModule, RouterModule, AuthFormWrapperComponent],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
})
export class ForgotPasswordComponent {
  forgotPasswordEmail: string = '';
  isError: boolean = false;
  errorMessage: string = '';

  login: string = environment.urlShared.login;
  confirmationRequired: string = environment.urlFrontend.confirmationRequired;

  constructor(private _authService: AuthService, private _router: Router) {}

  forgotPassword() {
    this._authService.forgotPassword(this.forgotPasswordEmail).subscribe({
      next: (message) => {
        this._router.navigate([this.confirmationRequired], {
          queryParams: { email: this.forgotPasswordEmail, type: 'otp' },
        });
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
      },
      complete: () => {},
    });
  }
}
