import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { User } from './../classes/user';
import { environment } from '../../environments/environment';
import { Register } from '../classes/register';
import { Login } from '../classes/login';
import { ResetPassword } from '../classes/reset-password';

@Injectable({
  providedIn: 'root'
})
export class UsersDataService {

  private _baseUrl = environment.urlApi.baseUrl;
  private _userUrl = environment.urlApi.userUrl;
  private _login = environment.urlShared.login;
  private _signIn = environment.urlShared.signIn;
  private _forgotPassword = environment.urlShared.forgotPassword;
  private _resetPassword = environment.urlShared.resetPassword;

  constructor(private _httpClient: HttpClient) { }

  getUsers(page: number = 1, username: string | null = null): Observable<User[]> {
    let url: string = `${this._baseUrl}${this._userUrl}?page=${page}`;
    if (username) {
      url = `${url}&username=${username}`;
    }

    return this._httpClient.get<{ message: string, data: User[] }>(url).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  getUser(userId: number): Observable<User> {
    let url: string = `${this._baseUrl}${this._userUrl}${userId}`;
    return this._httpClient.get<{ message: string, data: User }>(url).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  getToken(user: Login): Observable<string> {
    let url: string = `${this._baseUrl}${this._userUrl}`;
    url =  `${url}${this._login}`
    return this._httpClient.post<string>(url, user.jsonify()).pipe(
      catchError(this.handleError)
    );
  }

  getTokenOAuth(provider: string): void {
    window.location.href = `${this._baseUrl}${this._userUrl}signin-${provider.toLowerCase()}`;
  }

  handleOAuthCallback(): Observable<string | null> {
    return new Observable(observer => {
      const urlParams = new URLSearchParams(window.location.search);
      const token = urlParams.get('token');
      const error = urlParams.get('error');
      
      if (error) {
        observer.error(error);
      } else if (token) {
        observer.next(token);
        window.history.replaceState({}, document.title, window.location.pathname);
      } else {
        observer.next(null);
      }
      observer.complete();
    });
  }

  createUser(user: Register): Observable<string> {
    let url: string = `${this._baseUrl}${this._userUrl}`;
    return this._httpClient.post<{ message: string }>(url, user.jsonify()).pipe(
      map(response => response.message),
      catchError(this.handleError)
    );
  }

  forgotPassword(email: string): Observable<string> {
    let url: string = `${this._baseUrl}${this._userUrl}${this._forgotPassword}`;
    return this._httpClient.post<{ message: string }>(url, { email: email }).pipe(
      map(response => response.message),
      catchError(this.handleError)
    );
  }

  resetPassword(resetPassword: ResetPassword): Observable<string> {
    let url: string = `${this._baseUrl}${this._userUrl}${this._resetPassword}`;
    return this._httpClient.post<{ message: string }>(url, resetPassword.jsonify()).pipe(
      map(response => response.message),
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred.';

    if (error.status === 400) {
      if (error.error.errors) {
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
      errorMessage = error.error.message || 'Invalid email or password. Please try again.';
    }

    else if (error.status === 403) {
      errorMessage = error.error.message || 'You do not have permission to perform this action.';
    }

    // Handle 404 Not Found (e.g., user not found)
    else if (error.status === 404) {
      errorMessage = error.error.message || 'The requested resource was not found.';
    }

    // Handle 500 Internal Server Error
    else if (error.status === 500) {
      errorMessage = error.error.message || 'A server error occurred. Please try again later.';
    }

    console.error('HTTP Error:', error);

    return throwError(() => new Error(errorMessage));
  }
}