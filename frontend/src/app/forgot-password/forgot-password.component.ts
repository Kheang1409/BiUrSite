import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UsersDataService } from '../services/users-data.service';
import { environment } from '../../environments/environment';
import { Router } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  imports: [FormsModule, CommonModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {

  forgotPasswordEmail: string = '';
  isError: boolean = false;
  errorMessage: string = '';

  login: string = environment.urlShared.login;
  confirmationRequired: string = environment.urlFrontend.confirmationRequired;

  constructor(private _userService: UsersDataService, private _router: Router){}

  forgotPassword(){
    this._userService.forgotPassword(this.forgotPasswordEmail).subscribe({
      next: (message) => {
        this._router.navigate(
          [this.confirmationRequired], 
          {queryParams: { email: this.forgotPasswordEmail, type: 'otp' }
        });
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
      },
      complete: () => {

      },
    })
  }
}
