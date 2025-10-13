import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { Post } from '../models/posts/post';
import { Notification } from '../models/notifications/notification';
import { PostsDataService } from './posts-data.service';

@Injectable({
  providedIn: 'root',
})
export class SignalRService implements OnDestroy {
  private _baseUrl = environment.urlApi.baseUrl;
  private _notificationHub = environment.urlApi.notificationHub;
  private _feedHub = environment.urlApi.feedHub;

  private hubConnection: signalR.HubConnection;
  private feedHubConnection: signalR.HubConnection | null = null;

  public notifications$ = new BehaviorSubject<Notification[]>([]);
  public posts$ = new BehaviorSubject<Post[]>([]);
  public connectionState$ = new BehaviorSubject<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected
  );

  constructor(
    private _authService: AuthService,
    private _postService: PostsDataService
  ) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this._baseUrl}${this._notificationHub}`, {
        accessTokenFactory: () => {
          const userId = this._authService.getLocalToken();
          return userId || '';
        },
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onclose(() => {
      this.connectionState$.next(signalR.HubConnectionState.Disconnected);
    });

    this.feedHubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this._baseUrl}${this._feedHub}`, {
        accessTokenFactory: () => {
          const userId = this._authService.getLocalToken();
          return userId || '';
        },
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onreconnecting(() => {
      this.connectionState$.next(signalR.HubConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected(() => {
      this.connectionState$.next(signalR.HubConnectionState.Connected);
    });
  }

  public startConnection(): void {
    if (
      this.hubConnection.state === signalR.HubConnectionState.Connected ||
      this.hubConnection.state === signalR.HubConnectionState.Connecting
    ) {
      return;
    }

    this.hubConnection
      .start()
      .then(() => {
        this.connectionState$.next(signalR.HubConnectionState.Connected);
      })
      .catch((err) => {
        console.error('Error while starting SignalR connection: ', err);
        this.connectionState$.next(signalR.HubConnectionState.Disconnected);
      });

    if (
      this.feedHubConnection &&
      this.feedHubConnection.state !== signalR.HubConnectionState.Connected
    ) {
      this.feedHubConnection
        .start()
        .then(() => {
          this.connectionState$.next(signalR.HubConnectionState.Connected);
        })
        .catch((err) => {
          console.error('Error while starting Feed SignalR connection: ', err);
        });
    }
  }

  public addNotificationListener(): void {
    this.hubConnection.on(
      'ReceiveNotification',
      (notification: Notification) => {
        if (notification) {
          this.notifications$.next([notification]);
        } else {
          console.error('Received null or undefined notification.');
        }
      }
    );
  }

  public addPostListener(): void {
    const conn = this.feedHubConnection || this.hubConnection;
    conn.on('ReceivePost', (post: any) => {
      this.posts$.next([post as Post]);
    });
  }

  public stopConnection(): void {
    this.hubConnection
      .stop()
      .then(() => {
        this.connectionState$.next(signalR.HubConnectionState.Disconnected);
      })
      .catch((err) => {
        console.error('Error while stopping SignalR connection: ', err);
      });

    if (this.feedHubConnection) {
      this.feedHubConnection
        .stop()
        .then(() => {
          this.connectionState$.next(signalR.HubConnectionState.Disconnected);
        })
        .catch((err) => {
          console.error('Error while stopping Feed SignalR connection: ', err);
        });
    }
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}
