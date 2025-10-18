import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { User } from '../models/users/user';
import { environment } from '../../environments/environment';
import { Register } from '../models/users/register';

import { ErrorHandlingService } from './error-handling.service';
import { UserUpdate } from '../models/users/userUpdate';

@Injectable({
  providedIn: 'root',
})
export class UsersDataService {
  private _baseUrl = environment.urlApi.baseUrl;
  private _userUrl = environment.urlApi.userUrl;
  private queryPageNumber = environment.urlApi.query.pageNumber;

  private _me = environment.urlApi.me;

  constructor(
    private _httpClient: HttpClient,
    private _errorHandlingService: ErrorHandlingService
  ) {}

  getUsers(pageNumber: number = 1): Observable<User[]> {
    let url: string = `${this._baseUrl}${this._userUrl}?${this.queryPageNumber}=${pageNumber}`;
    return this._httpClient.get<any>(url).pipe(
      map((response) => response.data as User[]),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  getUser(userId: string): Observable<User> {
    let url: string = `${this._baseUrl}${this._userUrl}${userId}`;
    return this._httpClient.get<any>(url).pipe(
      map((response) => response.data as User),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  getProfile(): Observable<User> {
    let url: string = `${this._baseUrl}${this._userUrl}${this._me}`;
    return this._httpClient.get<any>(url).pipe(
      map((response) => response.data as User),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  createUser(user: Register): Observable<string> {
    let url: string = `${this._baseUrl}${this._userUrl}`;
    return this._httpClient.post<any>(url, user.jsonify()).pipe(
      map((response) => response.message ?? response.data?.message ?? ''),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  updateProfile(userUpdate: UserUpdate): Observable<string | null> {
    let url: string = `${this._baseUrl}${this._userUrl}${environment.urlApi.me}`;
    return this._httpClient
      .put<any>(url, userUpdate.toJSON())
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }

  markNotificationAsRead(): Observable<any> {
    let url: string = `${this._baseUrl}${this._userUrl}${environment.urlApi.me}`;
    return this._httpClient
      .patch<any>(url, {})
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }
}
