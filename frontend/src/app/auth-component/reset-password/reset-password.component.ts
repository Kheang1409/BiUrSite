import { Component } from '@angular/core';
import { ResetPassword } from '../../models/users/reset-password';
import { UsersDataService } from '../../services/users-data.service';
import { Router, RouterModule } from '@angular/router';
import { environment } from '../../../environments/environment';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { AuthFormWrapperComponent } from '../auth-form-wrapper/auth-form-wrapper.component';

@Component({
  selector: 'app-reset-password',
  imports: [CommonModule, FormsModule, RouterModule, AuthFormWrapperComponent],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css'],
})
export class ResetPasswordComponent {
  resetData: ResetPassword = new ResetPassword();
  errorMessage: string = '';
  isPasswordMismatch: boolean = false;
  isError: boolean = false;
  login: string = environment.urlShared.login;

  constructor(
    private _usersService: UsersDataService,
    private _authService: AuthService,
    private _router: Router
  ) {}

  resetPassword() {
    this.isPasswordMismatch = false;
    this.isError = false;

    if (!this.resetData.isPasswordMatched()) {
      this.errorMessage = 'Passwords do not match!';
      this.isPasswordMismatch = true;
      return;
    }

    this._authService.resetPassword(this.resetData).subscribe({
      next: () => {
        this._router.navigate([this.login]);
      },
      error: (error) => {
        this.errorMessage =
          error.message || 'An unexpected error occurred. Please try again.';
        this.isError = true;
      },
    });
  }
}
