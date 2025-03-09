import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Register } from '../classes/register';
import { UsersDataService } from '../services/users-data.service';
import { environment } from '../../environments/environment';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  user!: Register;

  createFailMessage: string = '';
  isCreateFail: boolean = false;
  isPasswordMismatch: boolean = false;

  login: string = environment.urlShared.login;

  constructor(private _usersService: UsersDataService, private _router: Router) {}

  ngOnInit(): void {
    this.user = new Register()
  }

  onSubmit() {
    this.isPasswordMismatch = false;
    if (!this.user.isPasswordMatched()) {
      this.createFailMessage = 'Passwords do not match!'
      this.isPasswordMismatch = true;
      return;
    }
    this._usersService.createUser(this.user).subscribe({
      next: (message) => {
        this._router.navigate(['/confirmation-required'], {
          queryParams: { email: this.user.email }
        });
      },
      error: (error) => {
        this.createFailMessage = error;
        this.isCreateFail = true;
      }
    });
  }
}