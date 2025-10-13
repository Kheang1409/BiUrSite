import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Post } from '../../models/posts/post';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostsDataService } from '../../services/posts-data.service';
import { TimeAgoPipe } from '../../pipes/time-ago.pipe';
import { UpdatePost } from '../../models/posts/updatePost';

@Component({
  selector: 'app-edit-post-modal',
  imports: [CommonModule, FormsModule, TimeAgoPipe],
  templateUrl: './edit-post-modal.component.html',
  styleUrls: ['./edit-post-modal.component.css'],
})
export class EditPostModalComponent implements OnInit {
  post!: Post;
  updatePost!: UpdatePost;
  userProfileImage!: string;
  isError: boolean = false;
  errorMessage!: string;
  isSaving: boolean = false;

  constructor(
    private _postService: PostsDataService,
    private dialogRef: MatDialogRef<EditPostModalComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: {
      post: Post;
      userPayload: any;
    }
  ) {}

  ngOnInit(): void {
    this.updatePost = new UpdatePost();
    this.post = this.data.post;
    this.updatePost.text = this.post.text;
  }

  closeModal() {
    this.dialogRef.close();
  }

  savePost() {
    if (!this.updatePost.text.trim()) {
      this.isError = true;
      this.errorMessage = 'Post description cannot be empty.';
      return;
    }
    this._postService.updatePost(this.post.id, this.updatePost).subscribe({
      next: () => {
        this.post.userProfile,
          this.post.id,
          (this.post.text = this.updatePost.text);
        this.dialogRef.close(this.post);
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage =
          error.message || 'An error occurred while updating the post.';
      },
    });
  }
}
