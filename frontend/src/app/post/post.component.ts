import {
  Component,
  HostListener,
  Input,
  OnInit,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PostModalComponent } from '../post-modal/post-modal.component';
import { CommonModule } from '@angular/common';
import { Post } from '../models/posts/post';
import { AuthService } from '../services/auth.service';
import { EditPostModalComponent } from '../shared/edit-post-modal/edit-post-modal.component';
import { TimeAgoPipe } from '../pipes/time-ago.pipe';
import { ConfirmDeletionDialogComponent } from '../shared/confirm-deletion-dialog/confirm-deletion-dialog.component';

@Component({
  selector: 'app-post',
  imports: [CommonModule, TimeAgoPipe],
  templateUrl: './post.component.html',
  styleUrls: ['./post.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PostComponent implements OnInit {
  @Input() post!: Post;
  @Output() deletePost = new EventEmitter<string>();

  isMenuOpen: boolean = false;
  userPayload: any;

  constructor(private _dialog: MatDialog, private _authService: AuthService) {
    _authService.userPayload$.subscribe((userPayload) => {
      this.userPayload = userPayload;
    });
  }

  ngOnInit(): void {}

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const isMenuClicked = target.closest('.post-menu') !== null;
    if (!isMenuClicked && this.isMenuOpen) {
      this.isMenuOpen = false;
    }
  }

  isOwner(): boolean {
    return this.userPayload && this.post.userId === this.userPayload.sub;
  }

  openPostModal(): void {
    this._dialog.open(PostModalComponent, {
      width: '680px',
      panelClass: 'custom-post-dialog',
      data: { post: this.post, userPayload: this.userPayload },
    });
  }

  openEditPostModal(event: Event): void {
    event.stopPropagation();
    const dialogRef = this._dialog.open(EditPostModalComponent, {
      width: '560px',
      panelClass: 'custom-edit-post-dialog',
      data: {
        post: this.post,
        userPayload: this.userPayload,
      },
    });

    dialogRef.afterClosed().subscribe((updatedPost: Post) => {
      if (updatedPost) {
        this.post = updatedPost;
      }
    });
  }

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.isMenuOpen = !this.isMenuOpen;
  }

  onDelete(event: Event): void {
    event.stopPropagation();
    const dialogRef = this._dialog.open(ConfirmDeletionDialogComponent, {
      width: '400px',
      data: { itemType: 'Post' },
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.deletePost.emit(this.post.id);
      }
    });
  }

  onReport(event: Event): void {
    event.stopPropagation();
    console.log('Reported post:', this.post.id);
  }
}
