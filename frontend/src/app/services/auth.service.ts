import { Injectable } from '@angular/core';
import { Token } from './../classes/token';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})


export class AuthService {

  private tokenKey = environment.keys.tokenKey;
  private currentTime = Math.floor(Date.now() / 1000);


  #token!: Token;

  get token() : Token{
    return this.#token;
  }

  set token(token: Token) {
    this.#token = token;
  }

  setToken(token: Token) {
    this.#token = token;
    localStorage.setItem(this.tokenKey, token.token);
  }


  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  clearToken() {
    localStorage.removeItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    return localStorage.getItem(this.tokenKey) !== null && !this.isExpiryToken();
  }

  isExpiryToken(): boolean {
    if(this.getUserPayload() == null)
      return false;
    if (this.getUserPayload().exp < this.currentTime) {
      this.clearToken();
      return true;
    }
    return false;
  }

  getUserPayload(): any{
    if(this.getToken())
      return jwtDecode(`${this.getToken()}`);
    else
      return null;
  }

  logout() {
    this.clearToken();
  }
}