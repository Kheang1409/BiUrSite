import { Component, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PostModalComponent } from '../post-modal/post-modal.component';
import { CommonModule } from '@angular/common';
import { CommentModalComponent } from '../comment-modal/comment-modal.component';
import { Post } from '../classes/post';

@Component({
  selector: 'app-post',
  imports: [CommonModule],
  templateUrl: './post.component.html',
  styleUrls: ['./post.component.css']
})
export class PostComponent implements OnInit {
  @Input() post!: Post;
  constructor(private dialog: MatDialog) {}
  userProfileImage!: string;

  ngOnInit(): void {
    this.userProfileImage = this.post.author.profile || 'assets/img/profile-default.svg';
  }
  openPostModal() {
    this.dialog.open(PostModalComponent, {
      width: '600px',
      data: { postId: this.post.postId }
    });
  }

  openCommentModal(event: Event) {
    event.stopPropagation(); 
    this.dialog.open(CommentModalComponent, {
      width: '400px',
      data: { postId: this.post.postId }
    });
  }
}