import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Post } from '../../models/posts/post';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EditPostModalComponent } from '../../shared/edit-post-modal/edit-post-modal.component';
import { ConfirmDeletionDialogComponent } from '../../shared/confirm-deletion-dialog/confirm-deletion-dialog.component';
import { CommonModule } from '@angular/common';
import { TimeAgoPipe } from '../../pipes/time-ago.pipe';
import { ImageLightboxComponent } from '../../shared/image-lightbox/image-lightbox.component';

@Component({
  selector: 'app-post-details',
  standalone: true,
  imports: [CommonModule, MatDialogModule, TimeAgoPipe],
  templateUrl: './post-details.component.html',
  styleUrls: ['./post-details.component.css'],
})
export class PostDetailsComponent {
  imageClass: 'portrait' | 'landscape' | null = null;

  constructor(private _dialog: MatDialog) {}

  @Input() post!: Post;
  @Input() userPayload: any;
  @Output() postUpdated = new EventEmitter<Post>();

  isMenuOpen = false;

  isOwner(): boolean {
    return this.userPayload && this.post.userId === this.userPayload.sub;
  }

  toggleMenu(event: Event) {
    event.stopPropagation();
    this.isMenuOpen = !this.isMenuOpen;
  }

  editPost(event: Event) {
    event.stopPropagation();
    const dialogRef = this._dialog.open(EditPostModalComponent, {
      width: '560px',
      panelClass: 'custom-edit-post-dialog',
      data: { post: this.post },
    });
    dialogRef.afterClosed().subscribe((updated) => {
      if (updated) this.postUpdated.emit(updated);
    });
  }

  deletePost(event: Event) {
    event.stopPropagation();
    const dialogRef = this._dialog.open(ConfirmDeletionDialogComponent, {
      width: '400px',
      data: { itemType: 'Post' },
    });
    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
      }
    });
  }

  reportPost(event: Event) {
    event.stopPropagation();
    console.log('Report post');
  }

  onImageLoad(ev: Event) {
    const img = ev.target as HTMLImageElement;
    if (!img) return;
    try {
      const isPortrait = img.naturalHeight > img.naturalWidth;
      this.imageClass = isPortrait ? 'portrait' : 'landscape';
    } catch (e) {
      this.imageClass = null;
    }
  }

  openImage(event: Event, src: string) {
    event.stopPropagation();
    this._dialog.open(ImageLightboxComponent, {
      panelClass: 'image-lightbox-dialog',
      data: { src },
      maxWidth: '100vw',
      maxHeight: '100vh',
    });
  }
}
