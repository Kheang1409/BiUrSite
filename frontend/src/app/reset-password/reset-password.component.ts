import { Component } from '@angular/core';
import { ResetPassword } from '../classes/reset-password';
import { UsersDataService } from '../services/users-data.service';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-reset-password',
  imports: [CommonModule, FormsModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent {

  resetData: ResetPassword = new ResetPassword();
  errorMessage: string = '';
  isPasswordMismatch: boolean = false;
  isError: boolean = false;
  login: string = environment.urlShared.login;

  constructor(private _usersService: UsersDataService, private _router: Router) {}

  resetPassword() {
    this.isPasswordMismatch = false;
    this.isError = false; 

    if (!this.resetData.isPasswordMatched()) {
      this.errorMessage = 'Passwords do not match!';
      this.isPasswordMismatch = true;
      return;
    }

    this._usersService.resetPassword(this.resetData).subscribe({
      next: (message) => {
        this._router.navigate([this.login]);
      },
      error: (error) => {
        this.errorMessage = error.message || 'An unexpected error occurred. Please try again.';
        this.isError = true;
      }
    });
  }
}