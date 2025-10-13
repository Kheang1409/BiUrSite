import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { HeaderComponent } from './header/header.component';
import { CommonModule } from '@angular/common';
import { ToTopComponent } from './to-top/to-top.component';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, ToTopComponent, CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  private _profile: string = environment.urlFrontend.profile;
  private _authRoutes = [
    environment.urlShared.login,
    environment.urlShared.register,
    environment.urlShared.forgotPassword,
    environment.urlShared.resetPassword,
    environment.urlFrontend.confirmationRequired,
  ];

  constructor(private _router: Router) {}

  isAuthPage(): boolean {
    return this._authRoutes.includes(this._router.url);
  }
}
