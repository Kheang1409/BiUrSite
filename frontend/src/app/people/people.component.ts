import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  OnDestroy,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { UsersDataService } from '../services/users-data.service';
import { SearchService } from '../services/search.service';
import { environment } from '../../environments/environment';
import { User } from '../models/users/user';

@Component({
  selector: 'app-people',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.css'],
})
export class PeopleComponent implements OnInit, OnDestroy {
  users: User[] = [];

  pageNumber: number = 1;
  isLoading: boolean = false;
  hasMore: boolean = true;
  isError: boolean = false;
  errorMessage: string = '';
  keyword: string = '';

  private scrollDebounceTimeout: any;
  private intersectionObserver?: IntersectionObserver;
  @ViewChild('infiniteAnchor', { static: false })
  infiniteAnchor?: ElementRef<HTMLDivElement>;

  private searchSubscription!: Subscription;

  constructor(
    private _usersService: UsersDataService,
    private _searchService: SearchService
  ) {}

  ngOnInit(): void {
    this.searchSubscription = this._searchService.searchKeyword$.subscribe(
      (keyword) => {
        this.keyword = keyword;
        this.users = [];
        this.pageNumber = 1;
        this.hasMore = true;
        this.getUsers(this.pageNumber);
      }
    );

    this.getUsers(this.pageNumber);
    this.setupIntersectionObserver();
    window.addEventListener('scroll', this.onScrollFallback.bind(this));
  }

  ngAfterViewInit(): void {
    this.setupIntersectionObserver();
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) this.searchSubscription.unsubscribe();
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
    sessionStorage.removeItem(environment.keys.pageNumberKey);
  }

  private setupIntersectionObserver(): void {
    try {
      this.intersectionObserver = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting && !this.isLoading && this.hasMore) {
              this.loadMore();
            }
          });
        },
        { root: null, rootMargin: '200px', threshold: 0.1 }
      );
      if (this.infiniteAnchor && this.infiniteAnchor.nativeElement) {
        this.intersectionObserver.observe(this.infiniteAnchor.nativeElement);
      }
    } catch (e) {
      console.warn('IntersectionObserver not available for PeopleComponent', e);
    }
  }

  onScrollFallback(): void {
    if (this.isLoading || !this.hasMore) return;
    clearTimeout(this.scrollDebounceTimeout);
    this.scrollDebounceTimeout = setTimeout(() => {
      const windowHeight = window.innerHeight;
      const documentHeight = document.documentElement.scrollHeight;
      const scrollTop = window.scrollY || document.documentElement.scrollTop;

      if (scrollTop + windowHeight >= documentHeight) {
        this.loadMore();
      }
    }, 200);
  }

  getUsers(page: number): void {
    this.isLoading = true;
    this._usersService.getUsers(page).subscribe({
      next: (users) => {
        if (!Array.isArray(users) || users.length === 0) {
          this.hasMore = false;
        } else {
          const converted = users.map((u) =>
            u instanceof User ? u : User.fromJson(u)
          );
          this.users = [...this.users, ...converted];
        }
        this.isError = false;
      },
      error: (err) => {
        this.isError = true;
        this.errorMessage = err.message ?? String(err);
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  loadMore(): void {
    if (this.isLoading || !this.hasMore) return;
    this.pageNumber++;
    sessionStorage.setItem(
      environment.keys.pageNumberKey,
      String(this.pageNumber)
    );
    this.getUsers(this.pageNumber);
  }
}
