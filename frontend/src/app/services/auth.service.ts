import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, map, Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';
import { Login } from '../models/users/login';
import { ResetPassword } from '../models/users/reset-password';
import { ErrorHandlingService } from './error-handling.service';
import { HttpClient } from '@angular/common/http';

interface JwtPayload {
  exp?: number;
  sub?: string;
  id?: string;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenKey = environment.keys.tokenKey;

  private readonly _baseUrl = environment.urlApi.baseUrl;
  private readonly _authUrl = environment.urlApi.authUrl;
  private readonly _login = environment.urlShared.login;
  private readonly _forgotPassword = environment.urlShared.forgotPassword;
  private readonly _resetPassword = environment.urlShared.resetPassword;

  private tokenSubject = new BehaviorSubject<string | null>(null);
  private userPayloadSubject = new BehaviorSubject<JwtPayload | null>(null);
  private loggedInSubject = new BehaviorSubject<boolean>(false);

  public readonly token$ = this.tokenSubject.asObservable();
  public readonly userPayload$ = this.userPayloadSubject.asObservable();
  public readonly isLoggedIn$ = this.loggedInSubject.asObservable();

  constructor(
    private http: HttpClient,
    private errorHandler: ErrorHandlingService
  ) {
    this.initializeAuthState();
  }

  /** Initializes auth state on app startup */
  private initializeAuthState(): void {
    const token = localStorage.getItem(this.tokenKey);
    if (token && !this.isTokenExpired(token)) {
      this.updateAuthState(token);
    } else {
      this.clearAuthState();
    }
  }

  /** Login flow: retrieve and set token */
  getToken(login: Login): Observable<string> {
    const url = `${this._baseUrl}${this._authUrl}${this._login}`;
    return this.http.post<any>(url, login.toJSON()).pipe(
      map((response) => {
        const token = response.data?.token ?? response.token ?? '';
        if (token) this.setToken(token);
        return token;
      }),
      catchError((err) => this.errorHandler.handleError(err))
    );
  }

  forgotPassword(email: string): Observable<string> {
    const url = `${this._baseUrl}${this._authUrl}${this._forgotPassword}`;
    return this.http.post<any>(url, { email }).pipe(
      map((response) => response.message ?? ''),
      catchError((err) => this.errorHandler.handleError(err))
    );
  }

  resetPassword(resetPassword: ResetPassword): Observable<string> {
    const url = `${this._baseUrl}${this._authUrl}${this._resetPassword}`;
    return this.http.post<any>(url, resetPassword.jsonify()).pipe(
      map((response) => response.message ?? ''),
      catchError((err) => this.errorHandler.handleError(err))
    );
  }

  /** Save token and update related subjects */
  setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
    this.updateAuthState(token);
  }

  /** Return current token */
  getLocalToken(): string | null {
    return this.tokenSubject.value;
  }

  /** Clear auth data */
  logout(): void {
    this.clearAuthState();
  }

  /** Returns current user id from payload */
  getCurrentUserId(): string | null {
    const payload = this.userPayloadSubject.value;
    return payload?.sub || payload?.id || null;
  }

  /** Check expiration dynamically */
  private isTokenExpired(token: string): boolean {
    try {
      const payload: JwtPayload = jwtDecode(token);
      if (payload.exp && payload.exp < Date.now() / 1000) {
        this.clearAuthState();
        return true;
      }
      return false;
    } catch {
      this.clearAuthState();
      return true;
    }
  }

  /** Decode + push updates to observables */
  private updateAuthState(token: string): void {
    try {
      const payload: JwtPayload = jwtDecode(token);
      this.tokenSubject.next(token);
      this.userPayloadSubject.next(payload);
      this.loggedInSubject.next(true);
    } catch (error) {
      console.error('Error decoding token:', error);
      this.clearAuthState();
    }
  }

  /** Clear all subjects and storage */
  private clearAuthState(): void {
    localStorage.removeItem(this.tokenKey);
    this.tokenSubject.next(null);
    this.userPayloadSubject.next(null);
    this.loggedInSubject.next(false);
  }
}
