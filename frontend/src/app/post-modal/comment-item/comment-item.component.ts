import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TimeAgoPipe } from '../../pipes/time-ago.pipe';
import { Comment } from '../../models/comments/comment';
import { AuthService } from '../../services/auth.service';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDeletionDialogComponent } from '../../shared/confirm-deletion-dialog/confirm-deletion-dialog.component';

@Component({
  selector: 'app-comment-item',
  standalone: true,
  imports: [CommonModule, FormsModule, TimeAgoPipe, MatDialogModule],
  templateUrl: './comment-item.component.html',
  styleUrls: ['./comment-item.component.css'],
})
export class CommentItemComponent implements OnInit {
  @Input() comment!: Comment;
  @Input() postOwnerId!: string;
  @Input() userPayload: any;
  @Output() deleted = new EventEmitter<string>();
  @Output() edited = new EventEmitter<Comment>();

  editing = false;
  tempText!: string;
  username!: string;

  isLoggedIn!: boolean;

  constructor(private _authService: AuthService, private _dialog: MatDialog) {
    _authService.isLoggedIn$.subscribe((loggedIn) => {
      this.isLoggedIn = loggedIn;
    });
  }

  ngOnInit(): void {
    this.username = this.userPayload?.unique_name;
  }

  isPostOwner(): boolean {
    return (
      !!this.userPayload &&
      String(this.postOwnerId) === String(this.userPayload.sub)
    );
  }

  isOwner(): boolean {
    return (
      !!this.userPayload &&
      String(this.comment.userId) === String(this.userPayload.sub)
    );
  }

  startEditing() {
    this.editing = true;
    this.tempText = this.comment.text;
  }

  saveEdit() {
    if (this.tempText.trim() === '') return;
    this.comment.text = this.tempText;
    this.edited.emit(this.comment);
    this.editing = false;
  }

  cancelEdit() {
    this.editing = false;
    this.tempText = '';
  }

  deleteComment() {
    // Use the shared Material confirmation dialog like PostComponent
    const dialogRef = this._dialog.open(ConfirmDeletionDialogComponent, {
      width: '400px',
      data: { itemType: 'Comment' },
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.deleted.emit(this.comment.id);
      }
    });
  }
}
