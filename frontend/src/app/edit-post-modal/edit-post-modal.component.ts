import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Post } from '../classes/post';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostsDataService } from '../services/posts-data.service';

@Component({
  selector: 'app-edit-post-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-post-modal.component.html',
  styleUrl: './edit-post-modal.component.css'
})
export class EditPostModalComponent implements OnInit{
  post!: Post;
  userProfileImage!: string;
  isError: boolean = false;
  errorMessage: string = '';

  constructor(
    private _postService: PostsDataService,
    private dialogRef: MatDialogRef<EditPostModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { postId: number },
  ) {}

  ngOnInit(): void {
    this.post = new Post();
    const postId = this.data.postId;
    this.getPost(postId);
  }

  closeModal() {
    this.dialogRef.close();
  }

  getPost(postId: number) {
    this._postService.getPost(postId).subscribe({
      next: (post) => {
        this.post = Object.assign(new Post(), post);
      },
      error: (error) => {
        alert(error.message);
      },
      complete: () => {
        this.userProfileImage = this.post.author.profile || 'assets/img/profile-default.svg';
      },
    });
  }

  savePost() {
    if (!this.post.description.trim()) {
      this.isError = true;
      this.errorMessage = "Post description cannot be empty.";
      return;
    }
    this._postService.updatePost(this.post.postId, this.post).subscribe({
      next: () => {
        this.dialogRef.close(this.post);
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message || "An error occurred while updating the post.";
      },
    });
  }
}
