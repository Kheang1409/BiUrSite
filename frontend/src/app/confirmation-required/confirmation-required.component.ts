import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-confirmation-required',
  imports: [],
  templateUrl: './confirmation-required.component.html',
  styleUrl: './confirmation-required.component.css'
})
export class ConfirmationRequiredComponent {
  email: string = 'example@gmail.com' ;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    // Read the email from the query parameters
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
    });
  }
}
