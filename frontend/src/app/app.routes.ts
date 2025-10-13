import { Routes } from '@angular/router';
import { FeedComponent } from './feed/feed.component';
import { environment } from '../environments/environment';
import { ErrorPageComponent } from './error-page/error-page.component';
import { ConfirmationRequiredComponent } from './confirmation-required/confirmation-required.component';
import { ProfileComponent } from './profile/profile.component';
import { LoginComponent } from './auth-component/login/login.component';
import { ForgotPasswordComponent } from './auth-component/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './auth-component/reset-password/reset-password.component';
import { RegisterComponent } from './auth-component/register/register.component';

const feed = environment.urlFrontend.feed;
const login = environment.urlShared.login;
const forgotPassword = environment.urlShared.forgotPassword;
const resetPassword = environment.urlShared.resetPassword;
const register = environment.urlShared.register;
const profile = environment.urlFrontend.profile;
const confirmationRequired = environment.urlFrontend.confirmationRequired;
const error = environment.urlFrontend.error;

export const routes: Routes = [
  {
    path: '',
    redirectTo: feed,
    pathMatch: 'full',
  },
  {
    path: feed,
    component: FeedComponent,
  },
  {
    path: login,
    component: LoginComponent,
  },
  {
    path: forgotPassword,
    component: ForgotPasswordComponent,
  },
  {
    path: register,
    component: RegisterComponent,
  },
  {
    path: profile,
    component: ProfileComponent,
  },
  {
    path: resetPassword,
    component: ResetPasswordComponent,
  },
  {
    path: confirmationRequired,
    component: ConfirmationRequiredComponent,
  },
  {
    path: error,
    component: ErrorPageComponent,
  },
];
