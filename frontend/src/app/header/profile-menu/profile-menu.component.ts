import { Component, OnInit, HostListener, Input } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { UsersDataService } from '../../services/users-data.service';
import { User } from '../../models/users/user';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-profile-menu',
  imports: [CommonModule],
  templateUrl: './profile-menu.component.html',
  styleUrls: ['./profile-menu.component.css'],
})
export class ProfileMenuComponent implements OnInit {
  @Input()
  user: User = new User();
  showProfileDropdown = false;
  isLoggedIn = false;

  private _profile: string = environment.urlFrontend.profile;

  constructor(
    private _authService: AuthService,
    private _usersService: UsersDataService,
    private _router: Router
  ) {
    _authService.isLoggedIn$.subscribe((loggedIn) => {
      this.isLoggedIn = loggedIn;
    });
  }

  ngOnInit(): void {}

  toggleProfileDropdown(event: Event) {
    event.stopPropagation();
    this.showProfileDropdown = !this.showProfileDropdown;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.showProfileDropdown) return;
    const target = event.target as HTMLElement;
    if (!target) return;
    const isInside = target.closest('.profile-menu') !== null;
    if (isInside) return;
    this.showProfileDropdown = false;
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent) {
    if (this.showProfileDropdown) this.showProfileDropdown = false;
  }

  toProfilePage() {
    this._router.navigate([this._profile]);
  }

  logout() {
    this._authService.logout();
    this._router.navigate(['/login']);
  }
}
