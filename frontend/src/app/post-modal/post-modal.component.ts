import { Component, Inject, ChangeDetectionStrategy } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Post } from '../models/posts/post';
import { CommonModule } from '@angular/common';
import { CommentsListComponent } from './comments-list/comments-list.component';
import { PostDetailsComponent } from './post-details/post-details.component';

@Component({
  standalone: true,
  selector: 'app-post-modal',
  imports: [CommonModule, PostDetailsComponent, CommentsListComponent],
  templateUrl: './post-modal.component.html',
  styleUrls: ['./post-modal.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PostModalComponent {
  post!: Post;
  userPayload: any;

  constructor(
    private dialogRef: MatDialogRef<PostModalComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: {
      post: Post;
      userPayload: any;
    }
  ) {
    this.post = data.post;
    this.userPayload = data.userPayload;
  }

  closeModal() {
    this.dialogRef.close();
  }

  onPostUpdated(updatedPost: Post) {
    this.post = updatedPost;
  }
}
