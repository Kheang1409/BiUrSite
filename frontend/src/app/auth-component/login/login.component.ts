import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ThirdPartyLoginComponent } from '../../shared/third-party-login/third-party-login.component';
import { Login } from '../../models/users/login';
import { AuthFormWrapperComponent } from '../auth-form-wrapper/auth-form-wrapper.component';

@Component({
  selector: 'app-login',
  standalone: true, // <-- add this
  imports: [
    CommonModule,
    FormsModule,
    ThirdPartyLoginComponent,
    RouterModule,
    AuthFormWrapperComponent,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  user: Login = new Login();
  token!: string;

  unauthorizedMessage: string = '';
  isUnauthorized: boolean = false;

  feed: string = environment.urlFrontend.feed;

  constructor(private _authService: AuthService, private _router: Router) {
    _authService.isLoggedIn$.subscribe((loggedIn) => {
      if (loggedIn) this._router.navigate([this.feed]);
    });
  }

  ngOnInit(): void {}

  login() {
    this._authService.getToken(this.user).subscribe({
      next: (token) => {
        this.isUnauthorized = false;
        this._authService.setToken(token);
        this._router.navigate([this.feed]);
      },
      error: (error) => {
        this.unauthorizedMessage = error.message;
        this.isUnauthorized = true;
      },
    });
  }
}
