import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { environment } from './../../environments/environment';
import { Post } from '../models/posts/post';
import { ErrorHandlingService } from './error-handling.service';
import { CreatePost } from '../models/posts/createPost';
import { UpdatePost } from '../models/posts/updatePost';
import { PostDetail } from '../models/posts/postDetail';

@Injectable({
  providedIn: 'root',
})
export class PostsDataService {
  private _baseUrl = environment.urlApi.baseUrl;
  private _postUrl = environment.urlApi.postUrl;

  private queryPageNumber = environment.urlApi.query.pageNumber;
  private queryKeyword = environment.urlApi.query.keywords;

  private queryTotalPost = environment.urlApi.total;

  constructor(
    private _httpClient: HttpClient,
    private _errorHandlingService: ErrorHandlingService
  ) {}

  getPosts(pageNumber: number, keyword: string | null): Observable<Post[]> {
    let url: string = `${this._baseUrl}${this._postUrl}?${this.queryPageNumber}=${pageNumber}`;
    if (keyword) {
      url = `${url}&${this.queryKeyword}=${keyword}`;
    }
    console.log(url);
    return this._httpClient.get<any>(url).pipe(
      map((response) => (response.data ?? []) as Post[]),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  getPost(postId: string): Observable<PostDetail> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}`;
    return this._httpClient.get<any>(url).pipe(
      map((response) => response.data as PostDetail),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  getTotalPost(): Observable<number> {
    let url: string = `${this._baseUrl}${this._postUrl}${this.queryTotalPost}`;

    return this._httpClient.get<any>(url).pipe(
      map((response) => response.data?.postCount ?? response.postCount ?? 0),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  createPost(post: CreatePost): Observable<void> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    return this._httpClient
      .post<any>(url, post.toJSON())
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }

  updatePost(postId: string, updatedPost: UpdatePost): Observable<Post> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}`;
    return this._httpClient
      .put<any>(url, updatedPost.toJSON())
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }

  deletePost(postId: string): Observable<void> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}`;

    return this._httpClient
      .delete<void>(url)
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }
}
