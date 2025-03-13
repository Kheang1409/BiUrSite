import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Post } from '../classes/post';
import { PostsDataService } from '../services/posts-data.service';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';
import { User } from '../classes/user';
import { Comment } from '../classes/comment';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-post-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './post-modal.component.html',
  styleUrls: ['./post-modal.component.css']
})
export class PostModalComponent implements OnInit{
  post!: Post;
  userProfileImage!: string;

  login: string = environment.urlShared.login;
  
  newComment: string = '';
  username: string = '';

  isError: boolean = false;
  errorMessage: string = '';
  
  constructor(
    private _authService: AuthService,
    private _postService: PostsDataService, 
    private dialogRef: MatDialogRef<PostModalComponent>, 
    @Inject(MAT_DIALOG_DATA) public data: { postId: number },
    private _router: Router){}

  ngOnInit(): void {
    this.post = new Post();
    const postId = this.data.postId;
    this.getPost(postId);
  }

  closeModal() {
    this.dialogRef.close();
  }

  getPost(postId: number){
    this._postService.getPost(postId).subscribe({
      next: (post) => {
        this.post = post;
      },
      error: (error) => {
        alert(error.message);
      },
      complete: () => {
        this.userProfileImage = this.post.author.profile || 'assets/img/profile-default.svg';
      },
    })
  }

  postComment() {
    if (this._authService.isLoggedIn()) {
        this.username = this._authService.getUserPayLoad().given_name;
    }

    if (!this.username.trim()) {
        this._router.navigate([this.login]);
        return;
    }

    if (!this.newComment.trim()) {
        this.isError = true;
        this.errorMessage = "Comment cannot be empty.";
        return;
    }

    const newComment = new Comment();
    newComment.description = this.newComment.trim();

    this._postService.createComment(this.post.postId, newComment).subscribe({
        next: (createdComment) => {
            createdComment.commenter = new User();
            createdComment.commenter.username = this.username;
            this.post.comments.unshift(createdComment);
            this.newComment = '';
            this.isError = false;
        },
        error: (error) => {
            this.isError = true;
            this.errorMessage = error.message || "An error occurred while posting the comment.";
        },
        complete: () => {

        },
    });
  }
}