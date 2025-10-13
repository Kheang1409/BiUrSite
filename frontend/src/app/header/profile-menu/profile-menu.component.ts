import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { UsersDataService } from '../../services/users-data.service';
import { User } from '../../models/users/user';

@Component({
  selector: 'app-profile-menu',
  imports: [CommonModule],
  templateUrl: './profile-menu.component.html',
  styleUrls: ['./profile-menu.component.css'],
})
export class ProfileMenuComponent implements OnInit {
  user: User = new User();
  showProfileDropdown = false;
  isLoggedIn = false;

  constructor(
    private _authService: AuthService,
    private _userService: UsersDataService,
    private _router: Router
  ) {
    _authService.isLoggedIn$.subscribe((loggedIn) => {
      this.isLoggedIn = loggedIn;
      if (this.isLoggedIn) this.toProfilePage();
    });
  }

  ngOnInit(): void {}

  toggleProfileDropdown(event: Event) {
    event.stopPropagation();
    this.showProfileDropdown = !this.showProfileDropdown;
  }

  toProfilePage() {
    this._userService.getProfile().subscribe({
      next: (user) => {
        this.user = user;
      },
      error: () => {},
    });
  }

  logout() {
    this._authService.logout();
    this._router.navigate(['/login']);
  }
}
