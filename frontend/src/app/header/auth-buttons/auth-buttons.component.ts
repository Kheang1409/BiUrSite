import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth-buttons',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './auth-buttons.component.html',
  styleUrls: ['./auth-buttons.component.css'],
})
export class AuthButtonsComponent {
  login = environment.urlShared.login;
  isLoggedIn = false;

  constructor(private authService: AuthService) {
    authService.isLoggedIn$.subscribe((loggedIn) => {
      this.isLoggedIn = loggedIn;
    });
  }
}
