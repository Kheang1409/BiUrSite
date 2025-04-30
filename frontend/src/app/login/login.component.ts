import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { environment } from '../../environments/environment';
import { UsersDataService } from '../services/users-data.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Login } from '../classes/login';
import { OAuthLoginComponent } from '../oauth-login/oauth-login.component';


@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, OAuthLoginComponent],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})

export class LoginComponent implements OnInit {
  
  user: Login = new Login();
  token!: string;

  unauthorizedMessage: string = '';
  isUnauthorized: boolean = false;

  feed: string = environment.urlFrontend.feed;
  forgetPassword: string = environment.urlShared.forgotPassword;
  register: string = environment.urlFrontend.register;

  constructor(
    private _usersService: UsersDataService,
    private _authService: AuthService,
    private _router: Router
  ) {}

  ngOnInit(): void {
    if (this._authService.isLoggedIn()) {
      this._router.navigate([this.feed]);
    } else {
      this._authService.clearToken();
    }
  }


  login() {
    this.getToken();
  }

  getToken() {
    this._usersService.getToken(this.user).subscribe(
      {
        next: (token) => {
          this.isUnauthorized = false;
          this._authService.setToken(token);
        },
        error: (error) => {
          this.unauthorizedMessage = error.message;
          this.isUnauthorized = true;
        },
        complete: () => {
          if (!this.isUnauthorized) {
            this.isUnauthorized = false;
            this._router.navigate([this.feed]);
          }
        }
      }
    );
  }

  onForgotPassword() {
    this._router.navigate([this.forgetPassword]);
  }
}