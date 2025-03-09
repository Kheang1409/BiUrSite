import { Routes } from '@angular/router';
import { FeedComponent } from './feed/feed.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { environment } from '../environments/environment';
import { ErrorPageComponent } from './error-page/error-page.component';
import { ConfirmationRequiredComponent } from './confirmation-required/confirmation-required.component';
import { ProfileComponent } from './profile/profile.component';


const feed = environment.urlFrontend.feed;
const login = environment.urlShared.login;
const register = environment.urlFrontend.register;
const profile = environment.urlFrontend.profile;
const confirmationRequired = environment.urlFrontend.confirmationRequired;
const error = environment.urlFrontend.error;

export const routes: Routes = [
    {
        path: "", redirectTo: feed, pathMatch: "full"
    },
    {
        path: feed, component: FeedComponent
    },
    {
        path: login, component: LoginComponent
    },
    {
        path: register, component: RegisterComponent
    },
    {
        path: profile, component: ProfileComponent
    },
    {
        path: confirmationRequired, component: ConfirmationRequiredComponent
    },
    {
        path: error, component: ErrorPageComponent
    }
];
