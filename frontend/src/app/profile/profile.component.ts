import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { UsersDataService } from '../services/users-data.service';
import { AuthService } from '../services/auth.service';
import { PostsDataService } from '../services/posts-data.service';
import { NotificationsDataService } from '../services/notifications-data.service';
import { User } from '../models/users/user';
import { UserUpdate } from '../models/users/userUpdate';
import { environment } from '../../environments/environment';
import { ProfileEditModalComponent } from './profile-edit-modal/profile-edit-modal.component';
import { ProfileLeftComponent } from './profile-left/profile-left.component';
import { ProfileCenterComponent } from './profile-center/profile-center.component';
import { ProfileRightComponent } from './profile-right/profile-right.component';
import { Post } from '../models/posts/post';
import { Notification } from '../models/notifications/notification';

@Component({
  selector: 'app-profile',
  imports: [
    CommonModule,
    FormsModule,
    ProfileEditModalComponent,
    ProfileLeftComponent,
    ProfileCenterComponent,
    ProfileRightComponent,
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css'],
})
export class ProfileComponent implements OnInit, OnDestroy {
  user: User = new User();

  username = '';
  bio = '';

  selectedImagePreview: string | null = null;
  selectedImageData: string | null = null;

  editMode = false;
  isSaving = false;
  isError = false;
  errorMessage = '';

  oldImage = '';

  page: number = 1;
  hasMorePosts: boolean = true;

  private destroy$ = new Subject<void>();

  posts: Post[] = [];
  isLoading = false;
  notifications: any[] = [];

  @ViewChild('profileLeft', { static: false })
  profileLeftRef?: ProfileLeftComponent;
  constructor(
    private _authService: AuthService,
    private userService: UsersDataService,
    private _postService: PostsDataService,
    private _notificationService: NotificationsDataService,
    private router: Router
  ) {
    this._authService.isLoggedIn$.subscribe((loggedIn) => {
      if (!loggedIn) {
        this.router.navigate([environment.urlFrontend.feed]);
      }
      this.loadProfile();
    });
  }

  ngOnInit(): void {}

  loadProfile(): void {
    this.userService
      .getProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          this.user = user;
          this.username = user.username;
          this.bio = user.bio ?? '';
          this.oldImage = user.profile;
          this.clearImageSelection();
          // reset pagination when loading profile
          this.page = 1;
          this.posts = [];
          this.hasMorePosts = true;
          this.getPosts(this.page);
          this.getNotifications();
        },
        error: (err) => console.error('Failed to load profile', err),
      });
  }

  getPosts(pageNumber: number): void {
    this.isLoading = true;
    this._postService.getMyPosts(pageNumber).subscribe({
      next: (posts) => {
        if (posts.length === 0) {
          this.hasMorePosts = false;
        } else {
          const converted = posts.map((p) =>
            p instanceof Post ? p : Post.fromJSON(p)
          );
          this.mergePosts(converted);
        }
        this.isError = false;
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  loadMoreProfilePosts(): void {
    if (this.isLoading || !this.hasMorePosts) return;
    this.page++;
    this.getPosts(this.page);
  }

  private getNotifications() {
    this._notificationService.getNotifications(1).subscribe({
      next: (notifications) => {
        const converted = notifications.map((n) =>
          n instanceof Notification ? n : Notification.fromJSON(n)
        );

        this.notifications = converted;
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
      },
    });
  }

  onDeletePost(postId: string): void {
    this.posts = this.posts.filter((p) => p.id !== postId);
    this._postService
      .deletePost(postId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {},
        error: (err) => {
          console.error('Failed to delete post', err);
        },
      });
  }

  toggleEdit(): void {
    this.editMode = !this.editMode;

    if (!this.editMode) {
      this.username = this.user.username;
      this.bio = this.user.bio ?? '';
      this.clearImageSelection();
      this.clearErrors();
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.resizeImage(file, 1024, 0.8)
      .then((dataUrl) => {
        this.selectedImagePreview = dataUrl;
        this.selectedImageData = dataUrl.split(',')[1];
        this.user.profile = dataUrl;
        this.clearErrors();
      })
      .catch((err) => {
        console.error('Image resize failed', err);
        this.clearImageSelection();
        this.setError('Failed to process image.');
      });
  }

  removeImage(): void {
    this.clearImageSelection();
    this.user.profile = this.oldImage;
  }

  private clearImageSelection(): void {
    this.selectedImagePreview = null;
    this.selectedImageData = null;
    if (this.profileLeftRef?.clearFileInput) {
      try {
        this.profileLeftRef.clearFileInput();
      } catch {}
    }
  }

  private resizeImage(
    file: File,
    maxSize: number,
    quality = 0.8
  ): Promise<string> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      const reader = new FileReader();

      reader.onload = (ev: any) => {
        img.onload = () => {
          const canvas = document.createElement('canvas');
          let { width, height } = img;

          if (width > height && width > maxSize) {
            height = Math.round((height * maxSize) / width);
            width = maxSize;
          } else if (height > maxSize) {
            width = Math.round((width * maxSize) / height);
            height = maxSize;
          }

          canvas.width = width;
          canvas.height = height;
          const ctx = canvas.getContext('2d');
          if (!ctx) return reject('Canvas not supported');

          ctx.drawImage(img, 0, 0, width, height);
          const dataUrl = canvas.toDataURL(file.type || 'image/jpeg', quality);
          resolve(dataUrl);
        };
        img.onerror = reject;
        img.src = ev.target.result as string;
      };

      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  }

  onChildSave(userUpdate: UserUpdate): void {
    this.isSaving = true;

    this.userService
      .updateProfile(userUpdate)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isSaving = false;
          this.editMode = false;
          this.clearImageSelection();
          this.loadProfile();
        },
        error: (err) => {
          this.isSaving = false;
          this.setError(err.message || 'Failed to update profile.');
        },
      });
  }

  private setError(message: string): void {
    this.isError = true;
    this.errorMessage = message;
  }

  private clearErrors(): void {
    this.isError = false;
    this.errorMessage = '';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private mergePosts(items: Post[]) {
    if (!Array.isArray(items) || items.length === 0) return;

    const incomingById = new Map<string, Post>();
    for (const it of items) {
      if (it && it.id) incomingById.set(it.id, it);
    }
    const updated = this.posts.map((p) => incomingById.get(p.id) ?? p);
    for (const it of items) {
      if (!updated.find((u) => u.id === it.id)) {
        updated.push(it);
      }
    }
    this.posts = updated;
  }
}
