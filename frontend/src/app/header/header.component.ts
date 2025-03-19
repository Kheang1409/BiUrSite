import { Component, HostListener, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { FormsModule } from '@angular/forms';
import { SearchService } from '../services/search.service';
import { CommonModule } from '@angular/common';
import { SignalRService } from '../services/signal-r.service';
import { Subscription } from 'rxjs';
import { Notification } from '../classes/notification';

@Component({
  selector: 'app-header',
  imports: [CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})
export class HeaderComponent implements OnInit {
  showNotificationDropdown = false;
  showProfileDropdown = false;
  searchKeyword: string = '';
  userProfileImage = '';
  notifications: Notification[] = new Array<Notification>();;
  
  feed: string = environment.urlFrontend.feed;
  login: string = environment.urlShared.login;
  profile: string = environment.urlFrontend.profile;
  hasNewNotifications = false;

  constructor(
    private _authService: AuthService,
    private _router: Router,
    private _searchService: SearchService,
    private _signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.searchKeyword = sessionStorage.getItem('searchKeyword') || '';
    if (this._authService.isLoggedIn()) {
      this.userProfileImage = this._authService.getUserPayload().profile;

      this._signalRService.startConnection();
      this._signalRService.addNotificationListener();

      this._signalRService.notifications$.subscribe((notifications) => {
        this.notifications = [...notifications, ...this.notifications];
        if(notifications.length > 0)
          this.hasNewNotifications = true;
        console.log('Notifications updated:', notifications);
      });
    }
  }

  onSearch(): void {
    const keyword = this.searchKeyword.trim();
    this._searchService.updateSearchKeyword(keyword);
    sessionStorage.setItem('searchKeyword', keyword);
  }

  toggleNotificationDropdown(event: Event): void {
    event.stopPropagation();
    this.showNotificationDropdown = !this.showNotificationDropdown;
    this.showProfileDropdown = false;

    if (this.showNotificationDropdown) {
      this.hasNewNotifications = false;
    }
  }

  toggleProfileDropdown(event: Event): void {
    event.stopPropagation();
    this.showProfileDropdown = !this.showProfileDropdown;
    this.showNotificationDropdown = false;
  }

  goToProfile(): void {
    this._router.navigate([this.profile]);
  }

  isLoggedIn(): boolean {
    return this._authService.isLoggedIn();
  }

  logout(): void {
    this._authService.logout();
    this._router.navigate([this.login]);
  }

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;

    // Close notification dropdown if clicked outside
    const notificationIcon = target.closest('.notification-icon');
    const notificationDropdown = target.closest('.notification-dropdown');
    if (!notificationIcon && !notificationDropdown && this.showNotificationDropdown) {
      this.showNotificationDropdown = false;
      this.notifications = []; // Clear notifications
      this._signalRService.notifications$.next([]); // Clear notifications in the service
    }

    // Close profile dropdown if clicked outside
    const profileMenu = target.closest('.profile-menu');
    const profileDropdown = target.closest('.profile-dropdown');
    if (!profileMenu && !profileDropdown && this.showProfileDropdown) {
      this.showProfileDropdown = false;
    }
  }
}