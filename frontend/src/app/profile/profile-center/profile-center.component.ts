import {
  Component,
  EventEmitter,
  Input,
  Output,
  AfterViewInit,
  ViewChild,
  ElementRef,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { PostComponent } from '../../post/post.component';
import { Post } from '../../models/posts/post';

@Component({
  selector: 'app-profile-center',
  standalone: true,
  imports: [CommonModule, PostComponent],
  templateUrl: './profile-center.component.html',
  styleUrls: ['./profile-center.component.css'],
})
export class ProfileCenterComponent implements AfterViewInit, OnDestroy {
  @Input() posts: Post[] = [];
  @Input() isLoading = false;

  @Output() deletePost = new EventEmitter<string>();
  @Output() loadMore = new EventEmitter<void>();

  @ViewChild('infiniteAnchor', { static: false })
  infiniteAnchor?: ElementRef<HTMLDivElement>;

  private intersectionObserver?: IntersectionObserver;

  onDelete(id: string) {
    this.deletePost.emit(id);
  }

  ngAfterViewInit(): void {
    try {
      this.intersectionObserver = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting && !this.isLoading) {
              this.loadMore.emit();
            }
          });
        },
        {
          root: null,
          rootMargin: '200px',
          threshold: 0.1,
        }
      );

      if (this.infiniteAnchor && this.infiniteAnchor.nativeElement) {
        this.intersectionObserver.observe(this.infiniteAnchor.nativeElement);
      }
    } catch (e) {}
  }

  ngOnDestroy(): void {
    try {
      this.intersectionObserver?.disconnect();
    } catch (e) {}
  }
}
