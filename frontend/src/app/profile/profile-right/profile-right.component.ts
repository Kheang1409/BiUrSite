import {
  Component,
  Input,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimeAgoPipe } from '../../pipes/time-ago.pipe';
import { Notification } from '../../models/notifications/notification';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotificationsDataService } from '../../services/notifications-data.service';
import { SignalRService } from '../../services/signal-r.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-profile-right',
  standalone: true,
  imports: [CommonModule, TimeAgoPipe],
  templateUrl: './profile-right.component.html',
  styleUrls: ['./profile-right.component.css'],
})
export class ProfileRightComponent {
  @Input() notifications: Notification[] = [];

  // pagination & loading
  notificationsPage = 1;
  isLoadingNotifications = false;
  hasMoreNotifications = true;

  private destroy$ = new Subject<void>();
  private intersectionObserver?: IntersectionObserver;

  constructor(
    private _notificationService: NotificationsDataService,
    private _authService: AuthService,
    private signalRService: SignalRService,
    private cdRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this._authService.isLoggedIn$
      .pipe(takeUntil(this.destroy$))
      .subscribe((loggedIn) => {
        if (loggedIn) {
          this.notificationsPage = 1;
          this.notifications = [];
          this.hasMoreNotifications = true;
          this.getNotifications(this.notificationsPage);
          this.setupSignalR();
        }
      });
  }

  ngAfterViewInit(): void {
    // set up an observer for infinite scroll inside the notifications list
    const root = document.querySelector('.notifications-list');
    const anchor = document.querySelector('#notificationsAnchor');
    if (anchor && root) {
      this.intersectionObserver = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (
              entry.isIntersecting &&
              !this.isLoadingNotifications &&
              this.hasMoreNotifications
            ) {
              this.loadMoreNotifications();
            }
          });
        },
        {
          root: root as Element,
          rootMargin: '100px',
          threshold: 0.1,
        }
      );
      this.intersectionObserver.observe(anchor as Element);
    }
  }

  private getNotifications(page = 1) {
    if (this.isLoadingNotifications || !this.hasMoreNotifications) return;
    this.isLoadingNotifications = true;
    this._notificationService.getNotifications(page).subscribe({
      next: (notifications) => {
        const converted = notifications.map((n) =>
          n instanceof Notification ? n : Notification.fromJSON(n)
        );

        if (!Array.isArray(converted) || converted.length === 0) {
          this.hasMoreNotifications = false;
        } else if (page === 1) {
          this.notifications = converted;
        } else {
          this.notifications = [...this.notifications, ...converted];
        }
      },
      error: (error) => {
        // keep component resilient: log and continue
        console.error('Failed loading notifications', error);
      },
      complete: () => {
        this.isLoadingNotifications = false;
        this.cdRef.detectChanges();
      },
    });
  }

  loadMoreNotifications() {
    if (this.isLoadingNotifications || !this.hasMoreNotifications) return;
    this.notificationsPage++;
    this.getNotifications(this.notificationsPage);
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
        this.mergeNotifications(converted);
        this.cdRef.detectChanges();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.intersectionObserver?.disconnect();
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
    this.notifications = updated;
  }
}
