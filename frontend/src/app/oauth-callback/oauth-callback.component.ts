import { Component, OnInit } from '@angular/core';
import { UsersDataService } from '../services/users-data.service';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-oauth-callback',
  imports: [],
  templateUrl: './oauth-callback.component.html',
  styleUrl: './oauth-callback.component.css'
})
export class OAuthCallbackComponent implements OnInit{

  constructor(
    private usersDataService: UsersDataService,
    private authDataService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
  this.usersDataService.handleOAuthCallback().subscribe({
    next: (token) => {
      if (token) {
        this.authDataService.setToken(token);
        this.router.navigate(['/feed']);
      } else {
        this.router.navigate(['/login']);
      }
    },
    error: (error) => {
      this.router.navigate(['/login'], { queryParams: { error } });
    }
  });
}
}
