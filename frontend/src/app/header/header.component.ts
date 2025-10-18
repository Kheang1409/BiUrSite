import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LogoComponent } from './logo/logo.component';
import { ActionIconsComponent } from './action-icons/action-icons.component';
import { ProfileMenuComponent } from './profile-menu/profile-menu.component';
import { AuthButtonsComponent } from './auth-buttons/auth-buttons.component';
import { SearchComponent } from './search-component/search-component.component';
import { AuthService } from '../services/auth.service';
import { UsersDataService } from '../services/users-data.service';
import { User } from '../models/users/user';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    LogoComponent,
    SearchComponent,
    ActionIconsComponent,
    ProfileMenuComponent,
    AuthButtonsComponent,
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})
export class HeaderComponent {
  user: User = new User();

  constructor(
    private _authService: AuthService,
    private _usersService: UsersDataService
  ) {
    _authService.isLoggedIn$.subscribe((loggedIn) => {
      if (loggedIn) this.getProfile();
    });
  }

  private getProfile() {
    this._usersService.getProfile().subscribe({
      next: (user) => {
        this.user = user;
      },
      error: () => {},
    });
  }
}
