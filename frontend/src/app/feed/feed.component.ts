import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { PostComponent } from '../post/post.component';
import { Post } from '../classes/post';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';
import { PostsDataService } from '../services/posts-data.service';
import { Subscription } from 'rxjs';
import { SearchService } from '../services/search.service';
import { FormsModule } from '@angular/forms';
import { User } from '../classes/user';
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
  total_page!: number;
  total_job!: number;

  isError: boolean = false;
  errorMessage: string = '';

  username: string = '';
  keyword: string = '';
  newPostDescription: string = '';

  isLoading: boolean = false;
  hasMorePosts: boolean = true;

  private searchSubscription!: Subscription;

  constructor(
    private _authService: AuthService,
    private _postService: PostsDataService,
    private _searchService: SearchService,
    private _router: Router
  ) {
    this.page =
      sessionStorage.getItem(this.pageNumberKey) == null
        ? 1
        : Number(sessionStorage.getItem(this.pageNumberKey));
  }

  ngOnInit(): void {
    if (this._authService.isLoggedIn()) {
      this.username = this._authService.getUserPayLoad().given_name;
    }

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
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  @HostListener('window:scroll', ['$event'])
  onScroll(): void {
    if (this.isLoading || !this.hasMorePosts) return;

    const windowHeight = window.innerHeight;
    const documentHeight = document.documentElement.scrollHeight;
    const scrollTop = window.scrollY || document.documentElement.scrollTop;

    if (scrollTop + windowHeight >= documentHeight) {
      this.loadMorePosts();
    }
  }

  getPosts(pageNumber: number, keyword: string): void {
    this.isLoading = true;
    this._postService.getPosts(pageNumber, keyword, null).subscribe({
      next: (posts) => {
        if (posts.length === 0) {
          this.hasMorePosts = false;
        } else {
          this.posts = [...this.posts, ...posts];
        }
        this.isError = false;
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  createPost() {
    if(!this.username.trim()){
      this._router.navigate([this.login]);
    }
    if (!this.newPostDescription.trim()) {
      return;
    }
    const newPost = new Post();
    newPost.description = this.newPostDescription.trim();
    
    this._postService.createPost(newPost).subscribe({
      next: (createdPost) => {
        createdPost.author = new User();
        createdPost.author.username = this.username;
        this.posts.unshift(createdPost);
        this.newPostDescription = '';
      },
      error: (error) => {
        this.isError = true;
        this.errorMessage = error.message;
      },
      complete: () => {

      },
    });
  }

  deletePost(postId: number){
    this._postService.deletePost(postId).subscribe({
      next: () => {
        this.posts = this.posts.filter(post => post.postId !== postId);
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
}