import {
  Component,
  Input,
  AfterViewInit,
  ViewChild,
  ElementRef,
  OnDestroy,
  OnInit,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { Comment } from '../../models/comments/comment';
import { CommentItemComponent } from '../comment-item/comment-item.component';
import { Post } from '../../models/posts/post';
import { CommentsDataService } from '../../services/comments-data.service';
import { CreateComment } from '../../models/comments/createComment';
import { AuthService } from '../../services/auth.service';
import { UpdateComment } from '../../models/comments/updateComment';

@Component({
  selector: 'app-comments-list',
  standalone: true,
  imports: [CommonModule, CommentItemComponent, FormsModule],
  templateUrl: './comments-list.component.html',
  styleUrls: ['./comments-list.component.css'],
})
export class CommentsListComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() post!: Post;
  @Input() userPayload: any;

  @ViewChild('infiniteAnchor', { static: false })
  infiniteAnchor?: ElementRef<HTMLDivElement>;

  comments: Comment[] = [];
  newComment = '';
  isLoadingComments = false;
  commentsPage = 1;
  hasMoreComments = true;
  isSending = false;
  isError = false;
  errorMessage = '';
  username = '';
  isLoggedIn!: boolean;

  private destroy$ = new Subject<void>();
  private intersectionObserver?: IntersectionObserver;

  constructor(
    private _authService: AuthService,
    private _commentService: CommentsDataService,
    private _cdr: ChangeDetectorRef
  ) {
    _authService.isLoggedIn$.subscribe((loggedIn) => {
      this.isLoggedIn = loggedIn;
    });
  }

  ngOnInit(): void {
    this.username = this.userPayload?.unique_name;
    this.getComments(this.commentsPage);
  }

  ngAfterViewInit(): void {
    if (this.infiniteAnchor) {
      this.intersectionObserver = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (
              entry.isIntersecting &&
              !this.isLoadingComments &&
              this.hasMoreComments
            ) {
              this.loadMoreComments();
            }
          });
        },
        {
          root: document.querySelector('.comments-list'),
          rootMargin: '100px',
          threshold: 0.1,
        }
      );
      this.intersectionObserver.observe(this.infiniteAnchor.nativeElement);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.intersectionObserver?.disconnect();
  }

  getComments(pageNumber: number): void {
    this.isLoadingComments = true;
    this._commentService.getComments(this.post.id, pageNumber, null).subscribe({
      next: (comments) => {
        const converted = comments.map((c) =>
          c instanceof Comment ? c : Comment.fromJSON(c)
        );
        if (converted.length === 0) {
          this.hasMoreComments = false;
        } else {
          this.comments = [...this.comments, ...converted];
        }
        this.isError = false;
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message || 'Failed to load comments.';
      },
      complete: () => {
        this.isLoadingComments = false;
        this._cdr.detectChanges();
      },
    });
  }

  loadMoreComments(): void {
    if (this.isLoadingComments || !this.hasMoreComments) return;
    this.commentsPage++;
    this.getComments(this.commentsPage);
  }

  postComment(): void {
    if (this.isNoComment()) return;
    const createComment = new CreateComment();
    createComment.text = this.newComment;
    this.isSending = true;

    this._commentService.createComment(this.post.id, createComment).subscribe({
      next: (comment) => {
        const converted =
          comment instanceof Comment ? comment : Comment.fromJSON(comment);
        this.comments.push(converted);
        this.newComment = '';
      },
      error: (err) => {
        this.isError = true;
        this.errorMessage = err.message || 'Failed to create comment.';
      },
      complete: () => {
        this.isSending = false;
        this._cdr.detectChanges();
      },
    });
  }

  isNoComment(): boolean {
    return this.newComment.trim().length === 0;
  }

  onCommentEdited(updatedComment: Comment) {
    const updatePayload = { text: updatedComment.text };
    const updateComment = new UpdateComment();
    updateComment.text = updatePayload.text;
    this._commentService
      .updateComment(this.post.id, updatedComment.id, updateComment)
      .subscribe({
        next: () => {
          const index = this.comments.findIndex(
            (c) => c.id === updatedComment.id
          );
          if (index > -1) this.comments[index] = updatedComment;
          this._cdr.detectChanges();
        },
        error: (err) => {
          this.isError = true;
          this.errorMessage = err.message || 'Failed to update comment.';
        },
      });
  }

  onCommentDeleted(commentId: string) {
    this._commentService.deleteComment(this.post.id, commentId).subscribe({
      next: () => {
        this.comments = this.comments.filter((c) => c.id !== commentId);
        this._cdr.detectChanges();
      },
      error: (err) => {
        this.isError = true;
        this.errorMessage = err.message || 'Failed to delete comment.';
      },
    });
  }
}
