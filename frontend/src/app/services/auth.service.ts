import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenKey = environment.keys.tokenKey;
  private currentTime = Math.floor(Date.now() / 1000);
  private loggedInSubject = new BehaviorSubject<boolean>(false);
  private tokenSubject = new BehaviorSubject<string | null>(null);

  // Public observables
  public isLoggedIn$: Observable<boolean> = this.loggedInSubject.asObservable();
  public token$: Observable<string | null> = this.tokenSubject.asObservable();

  constructor() {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const token = localStorage.getItem(this.tokenKey);
    if (token && !this.isTokenExpired(token)) {
      this.tokenSubject.next(token);
      this.loggedInSubject.next(true);
    } else {
      this.clearToken();
    }
  }

  setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
    this.tokenSubject.next(token);
    this.loggedInSubject.next(true);
    this.currentTime = Math.floor(Date.now() / 1000);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  clearToken(): void {
    localStorage.removeItem(this.tokenKey);
    this.tokenSubject.next(null);
    this.loggedInSubject.next(false);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return token !== null && !this.isTokenExpired(token);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = jwtDecode(token);
      if (payload.exp && payload.exp < this.currentTime) {
        this.clearToken();
        return true;
      }
      return false;
    } catch (error) {
      console.error('Error decoding token:', error);
      this.clearToken();
      return true;
    }
  }

  getUserPayload(): any {
    const token = this.getToken();
    if (!token) return null;
    
    try {
      return jwtDecode(token);
    } catch (error) {
      console.error('Error decoding token:', error);
      this.clearToken();
      return null;
    }
  }

  logout(): void {
    this.clearToken();
  }

  getCurrentUserId(): string | null {
    const payload = this.getUserPayload();
    return payload?.sub || payload?.id || null;
  }
}