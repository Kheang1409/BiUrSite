import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserUpdate } from '../../models/users/userUpdate';
import { User } from '../../models/users/user';

@Component({
  selector: 'app-profile-edit-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile-edit-modal.component.html',
  styleUrls: ['./profile-edit-modal.component.css'],
})
export class ProfileEditModalComponent implements OnInit {
  @Input() user: User = new User();
  @Output() save = new EventEmitter<UserUpdate>();
  @Output() cancel = new EventEmitter<void>();

  username = '';
  bio = '';
  selectedImagePreview: string | null = null;
  selectedImageData: string | null = null;

  isSaving = false;
  isError = false;
  errorMessage = '';

  @ViewChild('fileInput', { static: false })
  fileInputRef!: ElementRef<HTMLInputElement>;

  ngOnInit(): void {
    this.username = this.user.username;
    this.bio = this.user.bio ?? '';
    this.selectedImagePreview = null;
    this.selectedImageData = null;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.resizeImage(file, 1024, 0.8)
      .then((dataUrl) => {
        this.selectedImagePreview = dataUrl;
        this.selectedImageData = dataUrl.split(',')[1];
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
  }

  private clearImageSelection(): void {
    this.selectedImagePreview = null;
    this.selectedImageData = null;
    if (this.fileInputRef?.nativeElement) {
      this.fileInputRef.nativeElement.value = '';
    }
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
      (userUpdate as any).data = this.selectedImageData;
    }

    this.save.emit(userUpdate);
  }

  cancelEdit(): void {
    this.cancel.emit();
  }

  private setError(message: string): void {
    this.isError = true;
    this.errorMessage = message;
  }

  private clearErrors(): void {
    this.isError = false;
    this.errorMessage = '';
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
}
