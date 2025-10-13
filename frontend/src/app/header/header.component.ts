import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LogoComponent } from './logo/logo.component';
import { ActionIconsComponent } from './action-icons/action-icons.component';
import { ProfileMenuComponent } from './profile-menu/profile-menu.component';
import { AuthButtonsComponent } from './auth-buttons/auth-buttons.component';
import { SearchComponent } from './search-component/search-component.component';

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
export class HeaderComponent {}
