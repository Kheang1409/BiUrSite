import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Inject, OnInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Post } from '../classes/post';
import { PostsDataService } from '../services/posts-data.service';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';
import { User } from '../classes/user';
import { Comment } from '../classes/comment';
import { FormsModule } from '@angular/forms';
import { EditPostModalComponent } from '../edit-post-modal/edit-post-modal.component';
import { TimeAgoPipe } from '../pipes/time-ago.pipe';
import { ConfirmDeletionDialogComponent } from '../shared/confirm-deletion-dialog/confirm-deletion-dialog.component';

@Component({
  selector: 'app-post-modal',
  imports: [CommonModule, FormsModule, TimeAgoPipe],
  templateUrl: './post-modal.component.html',
  styleUrls: ['./post-modal.component.css']
})
export class PostModalComponent implements OnInit, AfterViewInit {
  post!: Post;
  comments: Comment[] = new Array<Comment>();
  login: string = environment.urlShared.login;

  newComment: string = '';
  username: string = '';

  isError: boolean = false;
  errorMessage: string = '';

  isMenuOpen: boolean = false;
  userPayload!: any;

  commentsPage: number = 1;

  isLoading: boolean = false;
  hasMoreComments: boolean = true;
  isLoadingComments: boolean = false;

  @ViewChild('commentsList') commentsList!: ElementRef;

  constructor(
    private _authService: AuthService,
    private _postService: PostsDataService,
    private dialogRef: MatDialogRef<PostModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { post: Post },
    private _router: Router,
    private _dialog: MatDialog
  ) {

    if (this._authService.isLoggedIn() && this._authService.getUserPayload() !== null){
      this.userPayload = this._authService.getUserPayload();
    }
  }

  ngOnInit(): void {
    this.post = this.data.post;
    this.getComments(this.post.postId, this.commentsPage);
  }

  ngAfterViewInit(): void {
    this.commentsList.nativeElement.addEventListener('scroll', () => this.onScroll());
  }

  onScroll(): void {
    const commentsList = this.commentsList.nativeElement;
    const scrollTop = commentsList.scrollTop;
    const scrollHeight = commentsList.scrollHeight;
    const clientHeight = commentsList.clientHeight;

    if (scrollTop + clientHeight >= scrollHeight - 10 && !this.isLoadingComments && this.hasMoreComments) {
      this.loadMoreComments();
    }
  }

  loadMoreComments(): void {
    if (this.isLoadingComments || !this.hasMoreComments) return;
    this.commentsPage++;
    this.getComments(this.post.postId, this.commentsPage);
  }

  closeModal() {
    this.dialogRef.close();
  }

  getComments(postId: number, pageNumber: number) {
    this._postService.getComments(postId, pageNumber, null).subscribe({
      next: (comments) => {
        if (comments.length === 0) {
          this.hasMoreComments = false;
        } else {
          this.comments = [...this.comments, ... comments];
        }
        this.isError = false;
      },
      error: (error) => {
        alert(error.message);
      },
      complete: () => {
        this.isLoadingComments = false;
      },
    });
  }

  postComment() {
    if (!this._authService.isLoggedIn()) {
      this._router.navigate([this.login]);
      return;
    }

    if (!this.newComment.trim()) {
      this.isError = true;
      this.errorMessage = 'Comment cannot be empty.';
      return;
    }

    const newComment = new Comment();
    newComment.description = this.newComment.trim();

    this._postService.createComment(this.post.postId, newComment).subscribe({
      next: (createdComment) => {
        createdComment.commenter = new User();
        createdComment.commenter.userId = this.userPayload.sub;
        createdComment.commenter.username = this.userPayload.given_name;
        this.comments.unshift(createdComment);
        this.newComment = '';
        this.isError = false;
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message || 'An error occurred while posting the comment.';
      },
      complete: () => {},
    });
  }

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.isMenuOpen = !this.isMenuOpen;
  }

  openEditPostModal(event: Event) {
    event.stopPropagation();
    const dialogRef = this._dialog.open(EditPostModalComponent, {
      width: '600px',
      data: { postId: this.post.postId },
    });

    dialogRef.afterClosed().subscribe((updatedPost) => {
      if (updatedPost) {
        this.post = updatedPost;
      }
    });
  }

  delete(event: Event) {
    event.stopPropagation();

    const dialogRef = this._dialog.open(ConfirmDeletionDialogComponent, {
      width: '400px',
      data: { itemType: 'Post' }
    });
  
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
          this._postService.deletePost(this.post.postId).subscribe({
            next: () => {
              this.dialogRef.close();
            },
            error: (error) => {
              alert(error.message);
            },
          });
        }
    });
  } 

  report(event: Event) {
    event.stopPropagation();
    console.log('report!');
  }

  isCommentOwner(comment: Comment): boolean {
    return  this._authService.isLoggedIn() && comment.commenter.userId == this._authService.getUserPayload().sub;
  }

  editComment(comment: Comment): void {
    comment.isEditing = true;
    comment.updatedDescription = comment.description;
  }

  saveEditedComment(comment: Comment): void {
    if (!comment.updatedDescription?.trim()) {
      alert('Comment cannot be empty.');
      return;
    }
    comment.description = comment.updatedDescription.trim();
    comment.isEditing = false;
    comment = Object.assign(new Comment(), comment);
    this._postService.updateComment(this.post.postId, comment.commentId, comment).subscribe({
      error: (error) => {
        alert('Failed to update comment: ' + error.message);
        comment.description = comment.updatedDescription;
      },
    });
  }

  cancelEdit(comment: Comment): void {
    comment.isEditing = false;
    comment.updatedDescription = comment.description;
  }

  deleteComment(comment: Comment) {

    const dialogRef = this._dialog.open(ConfirmDeletionDialogComponent, {
      width: '400px',
      data: { itemType: 'Post' }
    });
  
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this._postService.deleteComment(this.post.postId, comment.commentId).subscribe({
          next: () => {
              this.comments = this.comments.filter((c) => c.commentId !== comment.commentId);
          },
          error: (error) => {
              alert('Failed to delete comment: ' + error.message);
          },
        });
      }
    });
  }

  isOwner(): boolean{
    return this.userPayload && this.post.author.userId == this.userPayload.sub;
  }
}