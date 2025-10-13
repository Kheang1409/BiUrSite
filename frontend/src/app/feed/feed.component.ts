import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  Component,
  OnInit,
  OnDestroy,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { PostComponent } from '../post/post.component';
import { Post } from '../models/posts/post';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';
import { PostsDataService } from '../services/posts-data.service';
import { Subscription } from 'rxjs';
import { SearchService } from '../services/search.service';
import { SignalRService } from '../services/signal-r.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-feed',
  imports: [CommonModule, PostComponent, FormsModule],
  templateUrl: './feed.component.html',
  styleUrls: ['./feed.component.css'],
})
export class FeedComponent implements OnInit, OnDestroy {
  posts: Post[] = new Array<Post>();

  private pageNumberKey = environment.keys.pageNumberKey;
  login: string = environment.urlShared.login;

  page: number = 1;

  isError: boolean = false;
  errorMessage: string = '';

  username: string = '';
  keyword: string = '';
  newPostDescription: string = '';
  isLoading: boolean = false;
  hasMorePosts: boolean = true;

  userPayload: any;

  private scrollDebounceTimeout: any;
  private intersectionObserver?: IntersectionObserver;
  @ViewChild('infiniteAnchor', { static: false })
  infiniteAnchor?: ElementRef<HTMLDivElement>;

  private searchSubscription!: Subscription;
  private postsSubscription!: Subscription;

  private _profile: string = environment.urlFrontend.profile;

  constructor(
    private _authService: AuthService,
    private _postService: PostsDataService,
    private _searchService: SearchService,
    private _signalRService: SignalRService,
    private _router: Router
  ) {
    this.page =
      sessionStorage.getItem(this.pageNumberKey) == null
        ? 1
        : Number(sessionStorage.getItem(this.pageNumberKey));
    const urlFragment = window.location.hash;
    if (urlFragment && urlFragment.includes('token=')) {
      const token = this.extractToken(urlFragment);
      this._authService.setToken(token);
    }

    _authService.userPayload$.subscribe((userPayload) => {
      this.userPayload = userPayload;
    });
  }

  ngOnInit(): void {
    this.searchSubscription = this._searchService.searchKeyword$.subscribe(
      (keyword) => {
        this.keyword = keyword;
        this.posts = [];
        this.page = 1;
        this.hasMorePosts = true;
        this.getPosts(this.page, this.keyword);
      }
    );

    this.getPosts(this.page, this.keyword);

    this._signalRService.startConnection();
    this._signalRService.addPostListener();

    this.postsSubscription = this._signalRService.posts$.subscribe((posts) => {
      const incoming = Array.isArray(posts) ? posts : [posts as any];
      const converted = incoming
        .map((p: any) => (p instanceof Post ? p : Post.fromJSON(p)))
        .filter((p) => !!p && !!p.id);
      if (converted.length === 0) return;
      this.mergePosts(converted);
    });
    this.setupIntersectionObserver();
    window.addEventListener('scroll', this.onScrollFallback.bind(this));
  }

  ngAfterViewInit(): void {
    this.setupIntersectionObserver();
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) this.searchSubscription.unsubscribe();
    if (this.postsSubscription) this.postsSubscription.unsubscribe();

    window.removeEventListener('scroll', this.onScrollFallback.bind(this));

    if (
      this.intersectionObserver &&
      this.infiniteAnchor &&
      this.infiniteAnchor.nativeElement
    ) {
      try {
        this.intersectionObserver.unobserve(this.infiniteAnchor.nativeElement);
        this.intersectionObserver.disconnect();
      } catch (e) {}
    }

    sessionStorage.removeItem(this.pageNumberKey);
  }

  private setupIntersectionObserver(): void {
    try {
      this.intersectionObserver = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting && !this.isLoading && this.hasMorePosts) {
              this.loadMorePosts();
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
    } catch (e) {
      console.warn(
        'IntersectionObserver not available, falling back to scroll listener',
        e
      );
    }
  }

  onScrollFallback(): void {
    if (this.isLoading || !this.hasMorePosts) return;
    clearTimeout(this.scrollDebounceTimeout);
    this.scrollDebounceTimeout = setTimeout(() => {
      const windowHeight = window.innerHeight;
      const documentHeight = document.documentElement.scrollHeight;
      const scrollTop = window.scrollY || document.documentElement.scrollTop;

      if (scrollTop + windowHeight >= documentHeight) {
        this.loadMorePosts();
      }
    }, 200);
  }

  getPosts(pageNumber: number, keyword: string): void {
    this.isLoading = true;
    this._postService.getPosts(pageNumber, keyword).subscribe({
      next: (posts) => {
        if (posts.length === 0) {
          this.hasMorePosts = false;
        } else {
          const converted = posts.map((p) =>
            p instanceof Post ? p : Post.fromJSON(p)
          );
          this.mergePosts(converted);
        }
        this.isError = false;
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  private mergePosts(items: Post[]) {
    if (!Array.isArray(items) || items.length === 0) return;

    const incomingById = new Map<string, Post>();
    for (const it of items) {
      if (it && it.id) incomingById.set(it.id, it);
    }
    const updated = this.posts.map((p) => incomingById.get(p.id) ?? p);
    for (const it of items) {
      if (!updated.find((u) => u.id === it.id)) {
        updated.push(it);
      }
    }
    this.posts = updated;
  }

  deletePost(postId: string) {
    this._postService.deletePost(postId).subscribe({
      next: () => {
        this.posts = this.posts.filter((post) => post.id !== postId);
        console.log('Post deleted successfully');
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
      },
    });
  }

  loadMorePosts(): void {
    if (this.isLoading || !this.hasMorePosts) return;
    this.page++;
    this.getPosts(this.page, this.keyword);
  }

  private extractToken(fragment: string): string {
    const tokenParam = 'token=';
    const startIndex = fragment.indexOf(tokenParam) + tokenParam.length;
    const token = fragment.substring(startIndex);
    history.replaceState(
      null,
      '',
      window.location.pathname + window.location.search
    );
    return token || '';
  }

  toProfilePage() {
    this._router.navigate([this._profile]);
  }
}
