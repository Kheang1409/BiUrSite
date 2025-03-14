import { Component, HostListener, Input, OnInit, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PostModalComponent } from '../post-modal/post-modal.component';
import { CommonModule } from '@angular/common';
import { CommentModalComponent } from '../comment-modal/comment-modal.component';
import { Post } from '../classes/post';
import { AuthService } from '../services/auth.service';
import { EditPostModalComponent } from '../edit-post-modal/edit-post-modal.component';
import { TimeAgoPipe } from '../pipes/time-ago.pipe';
import { ConfirmDeletionDialogComponent } from '../shared/confirm-deletion-dialog/confirm-deletion-dialog.component';

@Component({
  selector: 'app-post',
  imports: [CommonModule, TimeAgoPipe],
  templateUrl: './post.component.html',
  styleUrls: ['./post.component.css']
})

export class PostComponent implements OnInit {
  @Input() post!: Post;
  @Output() deletePost = new EventEmitter<number>(); 

  isOwner: boolean = false;
  isMenuOpen: boolean = false;

  constructor(private _dialog: MatDialog, private _authService: AuthService) {}

  ngOnInit(): void {
    if (this._authService.isLoggedIn()) {
      this.isOwner = this.post.author.userId == this._authService.getUserPayload().sub;
    }
  }

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const isMenuClicked = target.closest('.post-menu') !== null;
    if (!isMenuClicked && this.isMenuOpen) {
      this.isMenuOpen = false;
    }
  }

  openPostModal(): void {
    this._dialog.open(PostModalComponent, {
      width: '600px',
      data: { post: this.post }
    });
  }

  openEditPostModal(event: Event): void {
    event.stopPropagation();
    const dialogRef = this._dialog.open(EditPostModalComponent, {
      width: '600px',
      data: { postId: this.post.postId }
    });

    dialogRef.afterClosed().subscribe((updatedPost: Post) => {
      if (updatedPost) {
        this.post = updatedPost; 
      }
    });
  }

  openCommentModal(event: Event): void {
    event.stopPropagation();
    this._dialog.open(CommentModalComponent, {
      width: '400px',
      data: { postId: this.post.postId }
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
      data: { itemType: 'Post' } 
    });
  
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.deletePost.emit(this.post.postId); 
      }
    });
  }

  onReport(event: Event): void {
    event.stopPropagation();
    console.log('Reported post:', this.post.postId);
  }
}