import { Component, EventEmitter, Output } from '@angular/core';
import { environment } from '../../environments/environment';
import { UsersDataService } from '../services/users-data.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { gapi } from 'gapi-script';

declare const FB: any;
declare global {
  interface Window {
    fbAsyncInit: () => void;
    _fbInitialized?: boolean;
  }
}

@Component({
  selector: 'app-oauth-login',
  imports: [],
  templateUrl: './oauth-login.component.html',
  styleUrl: './oauth-login.component.css'
})
export class OAuthLoginComponent {
  @Output() unauthorizedError = new EventEmitter<string>();

  constructor(
    private _usersService: UsersDataService,
    private _authService: AuthService,
    private _router: Router
  ) {}

  ngOnInit(): void {
    this.initializeGoogleAuth();
    this.initializeFacebookAuth();
  }

  initializeGoogleAuth() {
    gapi.load('auth2', () => {
      try {
        const auth2 = gapi.auth2.init({
          client_id: environment.oauth.google.clientId!,
          cookiepolicy: environment.oauth.google.cookiepolicy!,
          scope: environment.oauth.google.scope!
        });
        auth2.then(() => {
          // console.log(environment.oauth.google.message!) 
        });
      } catch (err) {
        // console.error(environment.oauth.google.error!, err);
      }
    });
  }

  initializeFacebookAuth() {
    if (window._fbInitialized) {
      console.log(environment.oauth.facebook.initialized!);
      return;
    }
  
    (function (d, s, id) {
      let js: any, fjs = d.getElementsByTagName(s)[0];
      if (d.getElementById(id)) return;
      js = d.createElement(s); js.id = id;
      js.src = environment.oauth.facebook.urlSDK!;
      js.onload = () => {
        window.fbAsyncInit = () => {
          FB.init({
            appId: environment.oauth.facebook.appId!,
            cookie: true,
            xfbml: true,
            version: environment.oauth.facebook.version!,
          });
          window._fbInitialized = true;
          // console.log(environment.oauth.facebook.initialized!);
        };
      };
      fjs.parentNode?.insertBefore(js, fjs);
    })(document, 'script', 'facebook-jssdk');
  }  
  

  onGoogleLogin() {
    const GoogleAuth = gapi.auth2.getAuthInstance();
    GoogleAuth.signIn().then((googleUser: any) => {
      const token = googleUser.getAuthResponse().id_token;
      this._usersService.loginWithOAuth('google', token).subscribe({
        next: (token) => {
          this._authService.setToken(token);
          this._router.navigate([environment.urlFrontend.feed]);
        },
        error: (error) => {
          console.error(environment.oauth.google.error, error);
          this.unauthorizedError.emit(error.message);
        }
      });
    }).catch((error: any) => {
      if (error.error === 'popup_closed_by_user') {
        // console.log('User closed the Google login popup');
      } else {
        // console.error(environment.oauth.google.error, error);
        this.unauthorizedError.emit(environment.oauth.google.error);
      }
    });
  }

  onFacebookLogin() {
    if (!window._fbInitialized) {
      // console.error(environment.oauth.facebook.notInitialized!);
      this.unauthorizedError.emit(environment.oauth.facebook.notInitialized!);
      return;
    }
  
    FB.getLoginStatus((response: any) => {
      if (response.status === 'connected') {
        this.getFacebookUserDetails(response.authResponse.accessToken);
      } else {
        FB.login(
          (loginResponse: any) => {
            if (loginResponse.authResponse) {
              this.getFacebookUserDetails(loginResponse.authResponse.accessToken);
            } else {
              this.unauthorizedError.emit(environment.oauth.facebook.error!);
            }
          },
          { scope: 'email' }
        );
      }
    }, { auth_type: 'reauthorize' });
  }
  
  getFacebookUserDetails(accessToken: string) {
    FB.api('/me', { fields: 'id,name,email,picture' }, (response: any) => {
      if (response && !response.error) {
        this._usersService.loginWithOAuth('facebook', accessToken).subscribe({
          next: (token) => {
            this._authService.setToken(token);
            this._router.navigate([environment.urlFrontend.feed]);
          },
          error: (error) => {
            // console.error(environment.oauth.facebook.error, error);
            this.unauthorizedError.emit(error.message);
          }
        });
      } else {
        // console.error(environment.oauth.facebook.error, response.error);
        this.unauthorizedError.emit(environment.oauth.facebook.error);
      }
    });
  }
}
