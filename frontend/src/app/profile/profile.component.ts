import { Component, OnInit, OnDestroy } from '@angular/core';
import { PostsDataService } from '../services/posts-data.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';
import { Location } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-profile',
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit, OnDestroy  {

  postCount!: number;
  username!: string;
  profile!: string;
  userPayload!: any;

  private destroy$ = new Subject<void>();

  constructor(
    private _authService: AuthService, 
    private _postService: PostsDataService, 
    private _router: Router,
    private _location: Location
  ) {
    if(this._authService.isLoggedIn() && this._authService.getUserPayload() !== null) {
      this.userPayload = this._authService.getUserPayload();
    }
  }

  ngOnInit(): void {
    if(this.userPayload){
      this.getTotalPost();
    }
    else {
      this._router.navigate([environment.urlFrontend.feed]);
    }
  }

  getTotalPost()  {
    this._postService.getTotalPost().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (count) => {
        this.postCount =  count;
        this.username = this.userPayload.given_name;
        this.profile = this.userPayload.profile;
      },
      error: (error) => {
        console.error('Error fetching total posts:', error);
        alert(error);
      }
    });
  }

  back(){
    this._location.back();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}