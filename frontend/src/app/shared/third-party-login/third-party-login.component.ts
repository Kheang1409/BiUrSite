import { Component } from '@angular/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-third-party-login',
  standalone: true,
  templateUrl: './third-party-login.component.html',
  styleUrls: ['./third-party-login.component.css'],
})
export class ThirdPartyLoginComponent {
  loginWith3rdParty(platform: string) {
    let redirectUrl = '';
    if (platform === 'Google') {
      redirectUrl =
        environment.urlApi.baseUrl +
        environment.urlApi.authUrl +
        environment.urlShared.google;
    } else if (platform === 'Facebook') {
      redirectUrl =
        environment.urlApi.baseUrl +
        environment.urlApi.authUrl +
        environment.urlShared.facebook;
    }
    if (redirectUrl) {
      window.location.href = redirectUrl;
    }
  }
}
