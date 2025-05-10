import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { Post } from '../classes/post';
import { Notification } from '../classes/notification';
import { PostsDataService } from './posts-data.service';

@Injectable({
  providedIn: 'root',
})
export class SignalRService implements OnDestroy {
  private _baseUrl = environment.urlApi.baseUrl;
  private _notificationHub = environment.urlApi.notificationHub;

  private hubConnection: signalR.HubConnection;

  public notifications$ = new BehaviorSubject<Notification[]>([]);
  public posts$ = new BehaviorSubject<Post[]>([]);
  public connectionState$ = new BehaviorSubject<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);

  constructor(
    private _authService: AuthService,
    private _postService: PostsDataService
  ) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this._baseUrl}${this._notificationHub}`, {
        accessTokenFactory: () => {
          const userId = this._authService.getToken()
          return userId || '';
        },
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onclose(() => {
      this.connectionState$.next(signalR.HubConnectionState.Disconnected);
    });

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
  }

  public addNotificationListener(): void {
    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      if (notification) {
        this.notifications$.next([notification]); 
      } else {
        console.error('Received null or undefined notification.');
      }
    });
  }

  public addPostListener(): void {
    this.hubConnection.on('ReceivePost', (post: any) => {
      if (post) {
        this._postService.getPost(post.id).subscribe({
          next: (newPost) => {
            this.posts$.next([newPost]);
          },
          error: (error) => {
            console.error('Error fetching post details:', error);
          },
        });
      } else {
        console.error('Received null or undefined post.');
      }
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
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}