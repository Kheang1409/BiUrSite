import { Component, Input } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PostModalComponent } from '../post-modal/post-modal.component';
import { CommonModule } from '@angular/common';
import { CommentModalComponent } from '../comment-modal/comment-modal.component';

@Component({
  selector: 'app-post',
  imports: [CommonModule],
  templateUrl: './post.component.html',
  styleUrls: ['./post.component.css']
})
export class PostComponent {
  @Input() post: any; // Input property to receive post data

  constructor(private dialog: MatDialog) {}

  // Open post modal
  openPostModal() {
    this.dialog.open(PostModalComponent, {
      data: { post: this.post, mode: 'post' }, // Pass post data and mode
      width: '600px', // Set modal width
    });
  }

  // Open comment modal
  openCommentModal(event: Event) {
    event.stopPropagation(); // Prevent post click event from triggering
    const dialogRef = this.dialog.open(CommentModalComponent, {
      width: '400px', // Set modal width
    });

    // Handle the result when the modal is closed
    dialogRef.afterClosed().subscribe((newComment: string) => {
      if (newComment) {
        console.log('New Comment:', newComment);
        // Add the new comment to the post's comments array
        this.post.comments.push({
          text: newComment,
          user: 'Current User', // Replace with actual user data
          timestamp: 'Just now',
        });
      }
    });
  }

  // Toggle like
  toggleLike(event: Event) {
    event.stopPropagation(); // Prevent post click event from triggering
    this.post.liked = !this.post.liked;
    this.post.likes += this.post.liked ? 1 : -1;
  }

  // Share post
  sharePost(event: Event) {
    event.stopPropagation(); // Prevent post click event from triggering
    const postLink = `https://biursite.com/post/${this.post.id}`; // Replace with actual link
    navigator.clipboard.writeText(postLink).then(() => {
      alert('Link copied to clipboard!');
    });
  }
}