import {
  Component,
  OnInit,
  ChangeDetectorRef,
  OnDestroy,
  ElementRef,
  HostListener,
  Input,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { CreatePostModalComponent } from '../../create-post-modal/create-post-modal.component';
import { NotificationsDataService } from '../../services/notifications-data.service';
import { SignalRService } from '../../services/signal-r.service';
import { AuthService } from '../../services/auth.service';
import { Notification } from '../../models/notifications/notification';
import { TimeAgoPipe } from '../../pipes/time-ago.pipe';
import { UsersDataService } from '../../services/users-data.service';

@Component({
  selector: 'app-action-icons',
  standalone: true,
  imports: [CommonModule, TimeAgoPipe],
  templateUrl: './action-icons.component.html',
  styleUrls: ['./action-icons.component.css'],
})
export class ActionIconsComponent implements OnInit, OnDestroy {
  @Input()
  hasNewNotifications = false;
  notifications: Notification[] = new Array<Notification>();
  showNotificationDropdown = false;
  isError = false;
  errorMessage = '';
  isLoggedIn = false;

  private destroy$ = new Subject<void>();

  constructor(
    private _authService: AuthService,
    private _notificationService: NotificationsDataService,
    private _usersService: UsersDataService,
    private signalRService: SignalRService,
    private cdRef: ChangeDetectorRef,
    private dialog: MatDialog,
    private hostRef: ElementRef
  ) {}

  ngOnInit(): void {
    this._authService.isLoggedIn$
      .pipe(takeUntil(this.destroy$))
      .subscribe((loggedIn) => {
        this.isLoggedIn = loggedIn;
        this.cdRef.detectChanges();
        if (loggedIn) {
          this.getNotifications();
          this.setupSignalR();
        }
      });
  }

  private getNotifications() {
    this._notificationService.getNotifications(1).subscribe({
      next: (notifications) => {
        const converted = notifications.map((n) =>
          n instanceof Notification ? n : Notification.fromJSON(n)
        );

        this.notifications = converted.slice(0, 5);
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
      },
    });
  }

  private setupSignalR() {
    this.signalRService.startConnection();
    this.signalRService.addNotificationListener();

    this.signalRService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe((notifications) => {
        const incoming = Array.isArray(notifications)
          ? notifications
          : [notifications as any];
        const converted = incoming
          .map((n: any) =>
            n instanceof Notification ? n : Notification.fromJSON(n)
          )
          .filter((p) => !!p && !!p.id);
        if (converted.length === 0) return;
        this.hasNewNotifications = converted.length > 0;
        this.mergeNotifications(converted);
        this.cdRef.detectChanges();
      });
  }

  toggleNotificationDropdown(event: Event) {
    event.stopPropagation();
    this.showNotificationDropdown = !this.showNotificationDropdown;
    if (this.showNotificationDropdown) {
      this.notifications.forEach((n) => {
        try {
          if (this.hasNewNotifications) this.markNotificationAsRead();
        } catch {}
      });
      this.cdRef.detectChanges();
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.showNotificationDropdown) return;

    const target = event.target as HTMLElement;
    if (!target) return;

    if (this.hostRef && this.hostRef.nativeElement.contains(target)) return;

    this.cdRef.detectChanges();
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent) {
    if (!this.showNotificationDropdown) return;
    this.showNotificationDropdown = false;
    this.cdRef.detectChanges();
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

  openNotification(notification: Notification) {
    this.showNotificationDropdown = false;
    this.cdRef.detectChanges();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalRService.stopConnection();
  }

  private mergeNotifications(items: Notification[]) {
    if (!Array.isArray(items) || items.length === 0) return;

    const incomingById = new Map<string, Notification>();
    for (const it of items) {
      if (it && it.id) incomingById.set(it.id, it);
    }
    const updated = this.notifications.map((n) => incomingById.get(n.id) ?? n);
    for (const it of items) {
      if (!updated.find((u) => u.id === it.id)) {
        updated.unshift(it);
      }
    }
    if (this.showNotificationDropdown) this.markNotificationAsRead();
    this.notifications = updated.slice(0, 5);
  }

  private markNotificationAsRead() {
    this._usersService.markNotificationAsRead().subscribe({
      next: () => {
        this.hasNewNotifications = false;
      },
      error: () => {},
    });
  }
}
