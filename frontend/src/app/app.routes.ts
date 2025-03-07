import { Routes } from '@angular/router';
import { FeedComponent } from './feed/feed.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';

export const routes: Routes = [
    {
        path: "", redirectTo: "home", pathMatch: "full"
    },
    {
        path: "home", component: FeedComponent
    },
    {
        path: "login", component: LoginComponent
    },
    {
        path: "register", component: RegisterComponent
    }
];
