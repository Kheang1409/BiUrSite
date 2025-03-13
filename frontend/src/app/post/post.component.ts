import { Component, HostListener, Input, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PostModalComponent } from '../post-modal/post-modal.component';
import { CommonModule } from '@angular/common';
import { CommentModalComponent } from '../comment-modal/comment-modal.component';
import { Post } from '../classes/post';
import { AuthService } from '../services/auth.service';
import { EditPostModalComponent } from '../edit-post-modal/edit-post-modal.component';

@Component({
  selector: 'app-post',
  imports: [CommonModule],
  templateUrl: './post.component.html',
  styleUrls: ['./post.component.css']
})
export class PostComponent implements OnInit {
  @Input() post!: Post;
  @Input() deletePost!: (postId: number) => void;
  @Input() editPost!: (postId: number) => void;

  isOwner!: boolean;
  userProfileImage!: string;
  isMenuOpen: boolean = false;

  constructor(private _dialog: MatDialog, private _authService: AuthService) {}

  ngOnInit(): void {
    this.userProfileImage = this.post.author.profile || 'assets/img/profile-default.svg';
    if(this._authService.isLoggedIn()){
      this.isOwner = this.post.author.userId == this._authService.getUserPayLoad().sub;
    }
  }

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const postMenu = target.closest('.post-menu');
    if (!postMenu && this.isMenuOpen) {
      this.isMenuOpen = false;
    }
  }

  openPostModal() {
    this._dialog.open(PostModalComponent, {
      width: '600px',
      data: { postId: this.post.postId }
    });
  }

  openEditPostModal(event: Event) {
    event.stopPropagation(); 
    const dialogRef = this._dialog.open(EditPostModalComponent, {
      width: '600px',
      data: { postId: this.post.postId }
    });
  
    dialogRef.afterClosed().subscribe(updatedPost => {
      if (updatedPost) {
        this.post = updatedPost;
      }
    });
  }

  openCommentModal(event: Event) {
    event.stopPropagation(); 
    this._dialog.open(CommentModalComponent, {
      width: '400px',
      data: { postId: this.post.postId }
    });
  }

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.isMenuOpen = !this.isMenuOpen;
  }

  delete(event: Event){
    event.stopPropagation();
    if (this.deletePost) {
      this.deletePost(this.post.postId);
    }
  }

  report(event: Event){
    event.stopPropagation();
    console.log("report!");
  }
}