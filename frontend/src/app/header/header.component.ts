import { Component, HostListener, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { FormsModule } from '@angular/forms';
import { SearchService } from '../services/search.service';
import { CommonModule } from '@angular/common';
import { SignalRService } from '../services/signal-r.service';
import { Notification } from '../classes/notification';
import { NotificationsDataService } from '../services/notifications-data.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, OnDestroy {
  showNotificationDropdown = false;
  showProfileDropdown = false;
  searchKeyword: string = '';
  userProfileImage = '';
  notifications: Notification[] = [];
  hasNewNotifications = false;
  isError = false;
  errorMessage = '';
  isLoggedIn = false;

  feed = environment.urlFrontend.feed;
  login = environment.urlShared.login;
  profile = environment.urlFrontend.profile;

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private notificationService: NotificationsDataService,
    private router: Router,
    private searchService: SearchService,
    private signalRService: SignalRService,
    private cdRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.searchKeyword = sessionStorage.getItem('searchKeyword') || '';

    this.authService.isLoggedIn$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(loggedIn => {
      this.isLoggedIn = loggedIn;
      setTimeout(() => this.cdRef.detectChanges(), 0);
      this.cdRef.detectChanges();
      if (loggedIn) {
        this.initializeLoggedInState();
      } else {
        this.cleanUpLoggedOutState();
      }
    });
  }

  private initializeLoggedInState(): void {
    const userPayload = this.authService.getUserPayload();
    if (userPayload) {
      this.userProfileImage = userPayload.profile;
    }

    this.getNotifications();
    this.setupSignalR();
  }

  private cleanUpLoggedOutState(): void {
    this.notifications = [];
    this.userProfileImage = '';
    this.hasNewNotifications = false;
    this.signalRService.stopConnection();
  }

  private setupSignalR(): void {
    this.signalRService.startConnection();
    this.signalRService.addNotificationListener();

    this.signalRService.notifications$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(notifications => {
      this.notifications = [...notifications, ...this.notifications].slice(0, 5);
      this.hasNewNotifications = notifications.length > 0;
      this.cdRef.detectChanges();
    });
  }

  onSearch(): void {
    const keyword = this.searchKeyword.trim();
    this.searchService.updateSearchKeyword(keyword);
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
    this.router.navigate([this.profile]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate([this.login]);
  }

  getNotifications(): void {
    this.notificationService.getNotifications().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (notifications) => {
        this.isError = false;
        this.notifications = notifications;
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
      }
    });
  }

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;

    if (!target.closest('.notification-icon') && 
        !target.closest('.notification-dropdown') && 
        this.showNotificationDropdown) {
      this.showNotificationDropdown = false;
      this.notifications = [];
      this.signalRService.notifications$.next([]);
    }

    if (!target.closest('.profile-menu') && 
        !target.closest('.profile-dropdown') && 
        this.showProfileDropdown) {
      this.showProfileDropdown = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalRService.stopConnection();
  }
}