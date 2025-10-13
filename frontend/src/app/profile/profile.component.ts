import {
  Component,
  ElementRef,
  OnInit,
  OnDestroy,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProfileEditModalComponent } from './profile-edit-modal.component';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import { UsersDataService } from '../services/users-data.service';
import { AuthService } from '../services/auth.service';
import { User } from '../models/users/user';
import { UserUpdate } from '../models/users/userUpdate';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, ProfileEditModalComponent],
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

  private destroy$ = new Subject<void>();

  @ViewChild('fileInput', { static: false })
  fileInputRef!: ElementRef<HTMLInputElement>;

  constructor(
    private _authService: AuthService,
    private userService: UsersDataService,
    private router: Router
  ) {
    _authService.isLoggedIn$.subscribe((loggedIn) => {
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
        },
        error: (err) => console.error('Failed to load profile', err),
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
        this.user.profile = dataUrl; // immediate avatar preview
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
    this.user.profile = this.oldImage; // reset to default avatar if removed
  }

  private clearImageSelection(): void {
    this.selectedImagePreview = null;
    this.selectedImageData = null;
    if (this.fileInputRef?.nativeElement) {
      this.fileInputRef.nativeElement.value = '';
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

  saveProfile(): void {
    if (!this.username.trim()) {
      this.setError('Username cannot be empty.');
      return;
    }

    this.isSaving = true;

    const userUpdate = new UserUpdate();
    userUpdate.username = this.username.trim();
    userUpdate.bio = this.bio.trim();
    if (this.selectedImageData) {
      (userUpdate as any).data = this.selectedImageData; // Ideally: extend UserUpdate type
    }

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

  // Handler invoked when child modal emits a save event
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
}
