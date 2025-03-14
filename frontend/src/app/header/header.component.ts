import { Component, HostListener, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { FormsModule } from '@angular/forms';
import { SearchService } from '../services/search.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  imports: [CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent implements OnInit {
  showNotificationDropdown = false;
  showProfileDropdown = false;
  searchKeyword: string = '';
  userProfileImage = '';


  feed: string = environment.urlFrontend.feed;
  login: string = environment.urlShared.login;
  profile: string = environment.urlFrontend.profile;


  constructor(
    private _authService: AuthService,
    private _router: Router,
    private _searchService: SearchService 
  ) {

  }

  ngOnInit(): void {
    this.searchKeyword = sessionStorage.getItem('searchKeyword') || '';
    if(this._authService.isLoggedIn())
      this.userProfileImage = this._authService.getUserPayload().profile;
  } 

  onSearch(): void {
    const keyword = this.searchKeyword.trim();
    this._searchService.updateSearchKeyword(keyword);
    sessionStorage.setItem('searchKeyword', keyword);
  }

  toggleNotificationDropdown(event: Event) {
    event.stopPropagation();
    this.showNotificationDropdown = !this.showNotificationDropdown;
    this.showProfileDropdown = false;
  }

  toggleProfileDropdown(event: Event) {
    event.stopPropagation();
    this.showProfileDropdown = !this.showProfileDropdown;
    this.showNotificationDropdown = false;
  }

  goToProfile(){
    this._router.navigate([this.profile]);
  }

  isLoggedIn(): boolean {
    return this._authService.isLoggedIn();
  }

  logout(): void {
      this._authService.logout();
      this._router.navigate([this.login]);
  }

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent) {
    const target = event.target as HTMLElement;

    const notificationIcon = target.closest('.notification-icon');
    const notificationDropdown = target.closest('.notification-dropdown');
    if (!notificationIcon && !notificationDropdown && this.showNotificationDropdown) {
      this.showNotificationDropdown = false;
    }

    const profileMenu = target.closest('.profile-menu');
    const profileDropdown = target.closest('.profile-dropdown');
    if (!profileMenu && !profileDropdown && this.showProfileDropdown) {
      this.showProfileDropdown = false;
    }
  }
}