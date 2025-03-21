import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';
import { Notification } from '../classes/notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationsDataService {
  private _baseUrl = environment.urlApi.baseUrl;
  private _notificationsUrl = environment.urlApi.notificationUrl;

  private queryIsRead = environment.urlApi.query.isRead;

  constructor(private _httpClient: HttpClient) { }

  getNotifications(isRead: boolean | null): Observable<Notification[]> {
    let url: string = `${this._baseUrl}${this._notificationsUrl}`;
    if (isRead)
      url = `${url}&${this.queryIsRead}=${isRead}`;
    return this._httpClient.get<{ message: string, data: Notification[] }>(url).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  getNotification(notificationId: number): Observable<Notification> {
    let url: string = `${this._baseUrl}${this._notificationsUrl}`;
    url = `${url}${notificationId}`;

    return this._httpClient.get<{ message: string, data: Notification }>(url).pipe(
      map(response => response.data), // Extract the `data` property
      catchError(this.handleError)
    );
  }

  updateNotification(notificationId: number): Observable<Notification> {
    let url: string = `${this._baseUrl}${this._notificationsUrl}`;
    url = `${url}${notificationId}`;

    return this._httpClient.patch<{ message: string, data: Notification }>(url, {}).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  deleteNotification(notificationId: number): Observable<void> {
    let url: string = `${this._baseUrl}${this._notificationsUrl}`;
    url = `${url}${notificationId}`;

    return this._httpClient.delete<{ message: string }>(url).pipe(
      map(() => {}),
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred.';
    if (error.status === 400) {
      if (error.error.errors) {
        // Extract and format validation errors
        const userFriendlyErrors = [];
        for (const field in error.error.errors) {
          if (error.error.errors.hasOwnProperty(field)) {
            const firstErrorMessage = error.error.errors[field][0];
            userFriendlyErrors.push(`${field}: ${firstErrorMessage}`);
          }
        }
        errorMessage = userFriendlyErrors.join(' | ');
      } else if (error.error.message) {
        errorMessage = error.error.message;
      }
    }
  
    else if (error.status === 401) {
      errorMessage = 'You are not authorized to perform this action. Please log in.';
    }
  
    else if (error.status === 403) {
      errorMessage = 'You do not have permission to perform this action.';
    }
  
    else if (error.status === 404) {
      errorMessage = 'The requested resource was not found.';
    }
  
    // Handle 500 Internal Server Error
    else if (error.status === 500) {
      errorMessage = 'A server error occurred. Please try again later.';
    }
  
    console.error('HTTP Error:', error);
  
    return throwError(() => new Error(errorMessage));
  }
}
