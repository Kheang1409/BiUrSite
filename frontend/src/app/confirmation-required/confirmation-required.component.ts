import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-confirmation-required',
  imports: [CommonModule],
  templateUrl: './confirmation-required.component.html',
  styleUrls: ['./confirmation-required.component.css']
})
export class ConfirmationRequiredComponent {
  email: string = 'example@gmail.com';
  confirmationType: string = 'email';

  login = environment.urlShared.login;
  resetPassword = environment.urlShared.resetPassword;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.confirmationType = params['type'] || 'email';
    });
  }
}