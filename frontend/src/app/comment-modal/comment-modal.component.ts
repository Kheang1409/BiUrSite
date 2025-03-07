import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-comment-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './comment-modal.component.html',
  styleUrl: './comment-modal.component.css'
})
export class CommentModalComponent {
  newComment = ''; // Holds the new comment text

  constructor(private dialogRef: MatDialogRef<CommentModalComponent>) {}

  // Submit the comment
  submitComment() {
    if (this.newComment.trim()) {
      this.dialogRef.close(this.newComment); // Close the modal and return the comment
    }
  }

  // Close the modal without submitting
  closeModal() {
    this.dialogRef.close();
  }
}
