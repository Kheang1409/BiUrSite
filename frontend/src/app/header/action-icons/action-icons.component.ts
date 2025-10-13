import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { CreatePostModalComponent } from '../../create-post-modal/create-post-modal.component';
import { NotificationsDataService } from '../../services/notifications-data.service';
import { SignalRService } from '../../services/signal-r.service';
import { AuthService } from '../../services/auth.service';
import { Notification } from '../../models/notifications/notification';

@Component({
  selector: 'app-action-icons',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './action-icons.component.html',
  styleUrls: ['./action-icons.component.css'],
})
export class ActionIconsComponent implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  showNotificationDropdown = false;
  hasNewNotifications = false;
  isError = false;
  errorMessage = '';
  isLoggedIn = false;

  private destroy$ = new Subject<void>();

  constructor(
    private _authService: AuthService,
    private notificationService: NotificationsDataService,
    private signalRService: SignalRService,
    private cdRef: ChangeDetectorRef,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this._authService.isLoggedIn$
      .pipe(takeUntil(this.destroy$))
      .subscribe((loggedIn) => {
        this.isLoggedIn = loggedIn;
        this.cdRef.detectChanges();
        if (loggedIn) this.initLoggedIn();
      });
  }

  private initLoggedIn() {
    this.getNotifications();
    this.setupSignalR();
  }

  private getNotifications() {
    // this.notificationService
    //   .getNotifications()
    //   .pipe(takeUntil(this.destroy$))
    //   .subscribe({
    //     next: (notifications) => {
    //       this.isError = false;
    //       this.notifications = notifications;
    //     },
    //     error: (err) => {
    //       this.isError = true;
    //       this.errorMessage = err.message;
    //     },
    //   });
  }

  private setupSignalR() {
    // this.signalRService.startConnection();
    // this.signalRService.addNotificationListener();
    // this.signalRService.notifications$
    //   .pipe(takeUntil(this.destroy$))
    //   .subscribe((notifications) => {
    //     this.notifications = [...notifications, ...this.notifications].slice(
    //       0,
    //       5
    //     );
    //     this.hasNewNotifications = notifications.length > 0;
    //     this.cdRef.detectChanges();
    //   });
  }

  toggleNotificationDropdown(event: Event) {
    // event.stopPropagation();
    // this.showNotificationDropdown = !this.showNotificationDropdown;
    // if (this.showNotificationDropdown) this.hasNewNotifications = false;
  }

  openCreatePostModal() {
    if (!this.isLoggedIn) return;
    this.dialog.open(CreatePostModalComponent, {
      width: '100%',
      maxWidth: '680px',
      panelClass: 'custom-create-post-dialog',
      autoFocus: false,
      restoreFocus: false,
      disableClose: true,
      backdropClass: 'custom-dialog-backdrop',
    });
  }

  trackById(index: number, item: Notification) {
    return item.postId;
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalRService.stopConnection();
  }
}
