import { Component, OnInit } from '@angular/core';
import { User } from '../classes/user';
import { PostsDataService } from '../services/posts-data.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-profile',
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit  {

  constructor(private _authService: AuthService, private _postService: PostsDataService, private _router: Router) {}

  postCount!: number;
  username!: string;
  profile!: string;
  
  ngOnInit(): void {
    if(!this._authService.isLoggedIn())
      this._router.navigate([environment.urlFrontend.feed]);
    this.loadUserProfile();
  }

  loadUserProfile(){
      const payload = this._authService.getUserPayLoad();
      this.username = payload.given_name;
      this.profile = payload.profile || 'assets/img/profile-default.svg';
      this.getTotalPost();
  }

  getTotalPost()  {
    this._postService.getTotalPost().subscribe({
      next: (count) => {
        this.postCount =  count;
      },
      error: (error) => {
        alert(error)
      },
      complete: () => {

      },
    });
  }
}
