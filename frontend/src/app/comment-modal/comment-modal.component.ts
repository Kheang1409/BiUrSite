import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { PostsDataService } from '../services/posts-data.service';
import { Comment } from '../classes/comment';

@Component({
    selector: 'app-comment-modal',
    imports: [CommonModule, FormsModule],
    templateUrl: './comment-modal.component.html',
    styleUrl: './comment-modal.component.css'
})
export class CommentModalComponent implements OnInit {
    postId!: number;

    newComment: string = '';
    username: string = '';

    login: string = environment.urlShared.login;

    isError: boolean = false; 
    errorMessage: string = ''; 

    constructor(
        private dialogRef: MatDialogRef<CommentModalComponent>,
        @Inject(MAT_DIALOG_DATA) public data: { postId: number },
        private _authService: AuthService,
        private _postService: PostsDataService,
        private _router: Router
    ) {}

    ngOnInit(): void {
        if (this._authService.isLoggedIn()) {
            this.username = this._authService.getUserPayload().given_name;
        }
        this.postId = this.data.postId;
    }

    submitComment() {
        // Reset error state
        this.isError = false;
        this.errorMessage = '';

        // Check if the user is logged in
        if (!this.username.trim()) {
            this._router.navigate([this.login]);
            return;
        }

        // Check if the comment is empty
        if (!this.newComment.trim()) {
            this.isError = true;
            this.errorMessage = "Comment cannot be empty.";
            return;
        }

        // Create the comment object
        const newComment = new Comment();
        newComment.description = this.newComment.trim();

        // Submit the comment
        this._postService.createComment(this.postId, newComment).subscribe({
            next: (comment) => {
                alert("Comment submitted successfully!");
                this.dialogRef.close(this.newComment); // Close the modal and return the new comment
            },
            error: (error) => {
                this.isError = true;
                this.errorMessage = error.message || "An error occurred while submitting the comment.";
            },
            complete: () => {
              
            },
        });
    }

    closeModal() {
        this.dialogRef.close();
    }
}