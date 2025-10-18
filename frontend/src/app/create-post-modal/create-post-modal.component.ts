import { Component, ElementRef, ViewChild, HostListener } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ImageLightboxComponent } from '../shared/image-lightbox/image-lightbox.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { PostsDataService } from '../services/posts-data.service';
import { AuthService } from '../services/auth.service';
import { CreatePost } from '../models/posts/createPost';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-create-post-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-post-modal.component.html',
  styleUrls: ['./create-post-modal.component.css'],
})
export class CreatePostModalComponent {
  private _ignoreDocumentClicks = true;
  text: string = '';
  selectedImagePreview: string | null = null;
  selectedImageData: string | null = null; // base64
  @ViewChild('fileInput', { static: false })
  fileInputRef!: ElementRef<HTMLInputElement>;
  rows: number = 5;
  isError: boolean = false;
  errorMessage!: string;
  isSaving: boolean = false;
  login: string = environment.urlShared.login;

  constructor(
    private dialogRef: MatDialogRef<CreatePostModalComponent>,
    private _postService: PostsDataService,
    private _authService: AuthService,
    private _dialog: MatDialog,
    private _router: Router,
    private hostRef: ElementRef
  ) {
    _authService.isLoggedIn$.subscribe((loggedIn) => {
      if (loggedIn) this._router.navigate([this.login]);
    });

    // Ignore document clicks that triggered opening this dialog.
    // The click that opens the dialog can bubble to document and immediately close it.
    // Use a short timeout so we only ignore the opening click.
    setTimeout(() => (this._ignoreDocumentClicks = false), 0);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    // If we're still ignoring initial clicks (the click that opened the dialog), do nothing
    if (this._ignoreDocumentClicks) return;

    // If clicked inside the dialog component, do nothing
    const target = event.target as HTMLElement;
    if (!target) return;
    try {
      if (this.hostRef && this.hostRef.nativeElement.contains(target)) return;
    } catch (e) {
      // ignore
    }

    // Otherwise close the dialog
    try {
      this.dialogRef.close();
    } catch (e) {}
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent) {
    try {
      this.dialogRef.close();
    } catch (e) {}
  }

  isBigTextarea(): boolean {
    const len = this.text?.trim()?.length ?? 0;
    return len > 0 && len < 20 && !this.selectedImageData;
  }

  onTextInput() {
    if (this.selectedImageData) {
      const lines = (this.text || '').split('\n').length;
      const approxWrappedLines = Math.ceil((this.text || '').length / 40);
      const needed = Math.max(lines, approxWrappedLines);
      this.rows = Math.min(Math.max(1, needed), 3);
      return;
    }
    this.rows = 5;
  }

  onImageSelected(event: any) {
    const file: File = event.target.files?.[0];
    if (!file) return;
    this.resizeImage(file, 1024, 0.8)
      .then((dataUrl) => {
        this.selectedImagePreview = dataUrl;
        this.selectedImageData = dataUrl.split(',')[1];
        this.rows = 1;
        this.isError = false;
      })
      .catch((err) => {
        console.error('Image resize failed', err);
        this.isError = true;
        this.errorMessage = 'Failed to process image.';
      });
  }

  removeImage() {
    this.selectedImagePreview = null;
    this.selectedImageData = null;
    this.rows = 5;
    try {
      const el = this.fileInputRef?.nativeElement;
      if (el) el.value = '';
    } catch (e) {}
  }

  openImagePreview(event: Event, src: string | null) {
    event.stopPropagation();
    if (!src) return;
    this._dialog.open(ImageLightboxComponent, {
      data: { src },
      panelClass: 'image-lightbox-dialog',
      maxWidth: '100vw',
      maxHeight: '100vh',
    });
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
          if (width > height) {
            if (width > maxSize) {
              height = Math.round((height *= maxSize / width));
              width = maxSize;
            }
          } else {
            if (height > maxSize) {
              width = Math.round((width *= maxSize / height));
              height = maxSize;
            }
          }
          canvas.width = width;
          canvas.height = height;
          const ctx = canvas.getContext('2d');
          if (!ctx) return reject('Canvas not supported');
          ctx.drawImage(img, 0, 0, width, height);
          const mime = file.type || 'image/jpeg';
          const dataUrl = canvas.toDataURL(mime, quality);
          resolve(dataUrl);
        };
        img.onerror = reject;
        img.src = ev.target.result as string;
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  }

  cancel() {
    this.dialogRef.close();
  }

  save() {
    if (!this.text.trim() && !this.selectedImageData) {
      this.isError = true;
      this.errorMessage = 'Post cannot be empty.';
      return;
    }

    this.isSaving = true;
    const post = new CreatePost();
    post.text = this.text.trim();
    if (this.selectedImageData) (post as any).data = this.selectedImageData;

    this._postService.createPost(post).subscribe({
      next: (created) => {
        this.isSaving = false;
        this.dialogRef.close(created);
      },
      error: (err) => {
        this.isSaving = false;
        this.isError = true;
        this.errorMessage = err.message || 'Failed to create post.';
      },
    });
  }
}
