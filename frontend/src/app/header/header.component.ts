import { Component, HostListener, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { FormsModule } from '@angular/forms';
import { SearchService } from '../services/search.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  imports: [CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent implements OnInit {
  showNotificationDropdown = false;
  showProfileDropdown = false;
  searchKeyword: string = '';

  userProfileImage!: string; 

  feed: string = environment.urlFrontend.feed;
  login: string = environment.urlShared.login;

  constructor(
    private _authService: AuthService,
    private _router: Router,
    private _searchService: SearchService 
  ) {}

  ngOnInit(): void {
    this.userProfileImage = this._authService.getUserPayLoad().profile || 'assets/img/profile-default.svg';
  } 

  onSearch(): void {
    this._searchService.updateSearchKeyword(this.searchKeyword.trim());
  }

  toggleNotificationDropdown(event: Event) {
    event.stopPropagation(); // Prevent event bubbling
    this.showNotificationDropdown = !this.showNotificationDropdown;
    this.showProfileDropdown = false; // Close profile dropdown if open
  }

  toggleProfileDropdown(event: Event) {
    event.stopPropagation(); // Prevent event bubbling
    this.showProfileDropdown = !this.showProfileDropdown;
    this.showNotificationDropdown = false; // Close notification dropdown if open
  }

  profile(){
    this._router.navigate([environment.urlFrontend.profile]);
  }

  isLoggedIn(): boolean {
    return this._authService.isLoggedIn();
  }

  logout(): void {
      this._authService.logout();
      this._router.navigate([this.login]);
  }

  // Listen for clicks outside the dropdown menus
  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent) {
    const target = event.target as HTMLElement;

    // Check if the click is outside the notification dropdown
    const notificationIcon = target.closest('.notification-icon');
    const notificationDropdown = target.closest('.notification-dropdown');
    if (!notificationIcon && !notificationDropdown && this.showNotificationDropdown) {
      this.showNotificationDropdown = false;
    }

    // Check if the click is outside the profile dropdown
    const profileMenu = target.closest('.profile-menu');
    const profileDropdown = target.closest('.profile-dropdown');
    if (!profileMenu && !profileDropdown && this.showProfileDropdown) {
      this.showProfileDropdown = false;
    }
  }
}