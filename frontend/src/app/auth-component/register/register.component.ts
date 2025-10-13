import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Register } from '../../models/users/register';
import { environment } from '../../../environments/environment';
import { UsersDataService } from '../../services/users-data.service';
import { ThirdPartyLoginComponent } from '../../shared/third-party-login/third-party-login.component';
import { AuthFormWrapperComponent } from '../auth-form-wrapper/auth-form-wrapper.component';

@Component({
  selector: 'app-register',
  imports: [
    CommonModule,
    FormsModule,
    ThirdPartyLoginComponent,
    AuthFormWrapperComponent,
    RouterModule,
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  user!: Register;

  createFailMessage: string = '';
  isCreateFail: boolean = false;
  isPasswordMismatch: boolean = false;

  confirmationRequired: string = environment.urlFrontend.confirmationRequired;

  constructor(
    private _usersService: UsersDataService,
    private _router: Router
  ) {}

  ngOnInit(): void {
    this.user = new Register();
  }

  onSubmit() {
    this.isPasswordMismatch = false;
    if (!this.user.isPasswordMatched()) {
      this.createFailMessage = environment.message.passwordMissedMatch;
      this.isPasswordMismatch = true;
      return;
    }
    this._usersService.createUser(this.user).subscribe({
      next: () => {
        this._router.navigate([this.confirmationRequired], {
          queryParams: { email: this.user.email, type: 'email' },
        });
      },
      error: (error) => {
        this.createFailMessage = error;
        this.isCreateFail = true;
      },
    });
  }
}
