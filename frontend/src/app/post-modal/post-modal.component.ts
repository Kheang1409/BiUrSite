import { CommonModule, NgFor, NgIf } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
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
import { fromEvent, Subject } from 'rxjs';
import { debounceTime, finalize, takeUntil, throttleTime } from 'rxjs/operators';
import { ScrollingModule } from '@angular/cdk/scrolling';

@Component({
  selector: 'app-post-modal',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TimeAgoPipe,
    NgFor,
    NgIf,
    ScrollingModule
  ],
  templateUrl: './post-modal.component.html',
  styleUrls: ['./post-modal.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PostModalComponent implements OnInit, AfterViewInit, OnDestroy {
  post!: Post;
  comments: Comment[] = [];
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

  private destroy$ = new Subject<void>();
  private menuSubject = new Subject<void>();
  private _isOwner: boolean | null = null;
  private _scrollSubscription: any;

  constructor(
    private _authService: AuthService,
    private _postService: PostsDataService,
    private dialogRef: MatDialogRef<PostModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { post: Post },
    private _router: Router,
    private _dialog: MatDialog,
    private cdRef: ChangeDetectorRef
  ) {
    if (this._authService.isLoggedIn() && this._authService.getUserPayload() !== null) {
      this.userPayload = this._authService.getUserPayload();
    }

    this.menuSubject.pipe(
      debounceTime(100),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.isMenuOpen = !this.isMenuOpen;
      this.cdRef.markForCheck();
    });
  }

  ngOnInit(): void {
    this.post = this.data.post;
    this.getComments(this.post.id, this.commentsPage);
  }

  ngAfterViewInit(): void {
    this._scrollSubscription = fromEvent(this.commentsList.nativeElement, 'scroll')
      .pipe(
        throttleTime(100),
        takeUntil(this.destroy$)
      )
      .subscribe(() => this.onScroll());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this._scrollSubscription) {
      this._scrollSubscription.unsubscribe();
    }
  }

  get isOwner(): boolean {
    if (this._isOwner === null) {
      this._isOwner = this.userPayload && this.post.author.id == this.userPayload.sub;
    }
    return this._isOwner ?? false;
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
    this.getComments(this.post.id, this.commentsPage);
  }

  closeModal() {
    this.dialogRef.close();
  }

  getComments(postId: number, pageNumber: number) {
    if (this.isLoadingComments) return;
    
    this.isLoadingComments = true;
    this._postService.getComments(postId, pageNumber).pipe(
      finalize(() => {
        this.isLoadingComments = false;
        this.cdRef.markForCheck();
      })
    ).subscribe({
      next: (comments) => {
        if (comments.length === 0) {
          this.hasMoreComments = false;
        } else {
          this.comments = [...this.comments, ...comments];
        }
        this.isError = false;
        this.cdRef.markForCheck();
      },
      error: (error) => {
        alert(error.message);
        this.cdRef.markForCheck();
      }
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
      this.cdRef.markForCheck();
      return;
    }

    const newComment = new Comment();
    newComment.description = this.newComment.trim();

    this._postService.createComment(this.post.id, newComment).subscribe({
      next: (createdComment) => {
        createdComment.commenter = new User();
        createdComment.commenter.id = this.userPayload.sub;
        createdComment.commenter.username = this.userPayload.given_name;
        this.comments = [createdComment, ...this.comments];
        this.newComment = '';
        this.isError = false;
        this.cdRef.markForCheck();
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message || 'An error occurred while posting the comment.';
        this.cdRef.markForCheck();
      }
    });
  }

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.menuSubject.next();
  }

  openEditPostModal(event: Event) {
    event.stopPropagation();
    const dialogRef = this._dialog.open(EditPostModalComponent, {
      width: '600px',
      data: { postId: this.post.id },
    });

    dialogRef.afterClosed().subscribe((updatedPost) => {
      if (updatedPost) {
        this.post = updatedPost;
        this.cdRef.markForCheck();
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
        this._postService.deletePost(this.post.id).subscribe({
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
    return this._authService.isLoggedIn() && comment.commenter.id == this._authService.getUserPayload().sub;
  }

  editComment(comment: Comment): void {
    this.comments = [...this.comments]; // Create new array reference
    comment.isEditing = true;
    comment.updatedDescription = comment.description;
    this.cdRef.markForCheck();
  }

  saveEditedComment(comment: Comment): void {
    if (!comment.updatedDescription?.trim()) {
      alert('Comment cannot be empty.');
      return;
    }
    comment.description = comment.updatedDescription.trim();
    comment.isEditing = false;
    comment = Object.assign(new Comment(), comment);
    this._postService.updateComment(this.post.id, comment.id, comment).subscribe({
      error: (error) => {
        alert('Failed to update comment: ' + error.message);
        comment.description = comment.updatedDescription;
        this.cdRef.markForCheck();
      },
    });
  }

  cancelEdit(comment: Comment): void {
    comment.isEditing = false;
    comment.updatedDescription = comment.description;
    this.cdRef.markForCheck();
  }

  deleteComment(comment: Comment) {
    const dialogRef = this._dialog.open(ConfirmDeletionDialogComponent, {
      width: '400px',
      data: { itemType: 'Comment' }
    });
  
    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this._postService.deleteComment(this.post.id, comment.id).subscribe({
          next: () => {
            this.comments = this.comments.filter((c) => c.id !== comment.id);
            this.cdRef.markForCheck();
          },
          error: (error) => {
            alert('Failed to delete comment: ' + error.message);
          },
        });
      }
    });
  }

  trackByCommentId(index: number, comment: Comment): number {
    return comment.id;
  }
}