import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, map, Observable } from 'rxjs';
import { Notification } from '../models/notifications/notification';
import { ErrorHandlingService } from './error-handling.service';

@Injectable({
  providedIn: 'root',
})
export class NotificationsDataService {
  private _baseUrl = environment.urlApi.baseUrl;
  private _notificationsUrl = '';

  constructor(
    private _httpClient: HttpClient,
    private _errorHandlingService: ErrorHandlingService
  ) {}

  getNotifications(): Observable<Notification[]> {
    let url: string = `${this._baseUrl}${this._notificationsUrl}`;
    console.log(`url: ${url}`);
    return this._httpClient
      .get<Notification[]>(url)
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }

  getNotification(notificationId: number): Observable<Notification> {
    let url: string = `${this._baseUrl}${this._notificationsUrl}`;
    url = `${url}${notificationId}`;

    return this._httpClient
      .get<Notification>(url)
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }

  updateNotification(notificationId: number): Observable<Notification> {
    let url: string = `${this._baseUrl}${this._notificationsUrl}`;
    url = `${url}${notificationId}`;

    return this._httpClient
      .patch<Notification>(url, {})
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }

  deleteNotification(notificationId: number): Observable<void> {
    let url: string = `${this._baseUrl}${this._notificationsUrl}`;
    url = `${url}${notificationId}`;

    return this._httpClient
      .delete<void>(url)
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }

  // ...existing code...
}
