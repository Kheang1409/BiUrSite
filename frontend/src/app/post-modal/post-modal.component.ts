import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-post-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './post-modal.component.html',
  styleUrls: ['./post-modal.component.css']
})
export class PostModalComponent {
  post: any; // Post data
  mode: 'post' | 'comment'; // Modal mode
  newComment = ''; // New comment text

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<PostModalComponent>
  ) {
    this.post = data.post;
    this.mode = data.mode;
  }

  comments = [
    {
      text: 'This is awesome! 😍',
      user: 'Jane Smith',
      timestamp: '1 hour ago'
    },
    {
      text: 'Can\'t wait to try it out!',
      user: 'Alice Johnson',
      timestamp: '45 minutes ago'
    },
    {
      text: 'Great work, team! 👏',
      user: 'Bob Brown',
      timestamp: '30 minutes ago'
    }
  ];

  closeModal() {
    this.dialogRef.close();
  }

  toggleLike() {
    this.post.liked = !this.post.liked;
    this.post.likes += this.post.liked ? 1 : -1;
  }

  openCommentModal() {
    this.mode = 'comment'; // Switch to comment mode
  }

  postComment() {
    if (this.newComment.trim()) {
      this.post.comments.push({
        text: this.newComment,
        user: 'Current User', // Replace with actual user
        timestamp: new Date().toLocaleString()
      });
      this.newComment = ''; // Clear the input field
    }
  }

  sharePost() {
    const postLink = `https://biursite.com/post/${this.post.id}`; // Replace with actual link
    navigator.clipboard.writeText(postLink).then(() => {
      alert('Link copied to clipboard!');
    });
  }
}