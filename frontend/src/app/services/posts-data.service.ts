import { HttpClient, HttpErrorResponse} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { environment } from './../../environments/environment';
import { Post } from '../classes/post';
import { Comment } from '../classes/comment';

@Injectable({
  providedIn: 'root'
})
export class PostsDataService {

  private _baseUrl = environment.urlApi.baseUrl;
  private _postUrl = environment.urlApi.postUrl;
  private _subsetUrl = environment.urlApi.subsetUrl;

  private queryPageNumber = environment.urlApi.query.pageNumber;
  private queryKeyword = environment.urlApi.query.keyword;
  private queryUserId = environment.urlApi.query.userId;

  private queryTotalPost = environment.urlApi.totalPost;

  constructor(private _httpClient: HttpClient) { }

  getPosts(pageNumber: number, keyword: string | null, userId: string | null): Observable<Post[]> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}?${this.queryPageNumber}=${pageNumber}`;
    if (keyword)
      url = `${url}&${this.queryKeyword}=${keyword}`;
    if (userId)
      url = `${url}&${this.queryUserId}=${userId}`;

    return this._httpClient.get<{ message: string, data: Post[] }>(url).pipe(
      map(response => response.data), // Extract the `data` property
      catchError(this.handleError)
    );
  }

  getPost(postId: number): Observable<Post> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}${postId}`;

    return this._httpClient.get<{ message: string, data: Post }>(url).pipe(
      map(response => response.data), // Extract the `data` property
      catchError(this.handleError)
    );
  }

  getTotalPost(): Observable<number>{
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url =`${url}${this.queryTotalPost}`
    return this._httpClient.get<{postCount: number}>(url).pipe(
      map(response => response.postCount),
      catchError(this.handleError)
    );
  }

  createPost(newPost: Post): Observable<Post> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    return this._httpClient.post<{ message: string, data: Post }>(url, newPost.jsonify()).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  updatePost(postId: number, updatedPost: Post): Observable<Post> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}${postId}`;

    return this._httpClient.put<{ message: string, data: Post }>(url, updatedPost.jsonify()).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  deletePost(postId: number): Observable<void> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}${postId}`;

    return this._httpClient.delete<{ message: string }>(url).pipe(
      map(() => {}),
      catchError(this.handleError)
    );
  }

  getComments(postId: number, pageNumber: number, keyword: string | null, userId: string | null): Observable<Comment[]> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}${postId}${this._subsetUrl}?${this.queryPageNumber}=${pageNumber}`;
    if (keyword)
      url = `${url}&${this.queryKeyword}=${keyword}`;
    if (userId)
      url = `${url}&${this.queryUserId}=${userId}`;

    return this._httpClient.get<{ message: string, data: Comment[] }>(url).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  createComment(postId: number, newComment: Comment): Observable<Comment> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}${postId}${this._subsetUrl}`;
    return this._httpClient.post<{ message: string, data: Comment }>(url, newComment.jsonify()).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  updateComment(postId: number, commentId: number, updateComment: Comment){
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}${postId}${this._subsetUrl}${commentId}`;
    return this._httpClient.put<{ message: string, data: Comment }>(url, updateComment.jsonify()).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  deleteComment(postId: number, commentId: number){
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}${postId}${this._subsetUrl}${commentId}`;
    return this._httpClient.delete<{ message: string, data: Comment }>(url).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred.';
    if (error.status === 400) {
      if (error.error.errors) {
        // Extract and format validation errors
        const userFriendlyErrors = [];
        for (const field in error.error.errors) {
          if (error.error.errors.hasOwnProperty(field)) {
            const firstErrorMessage = error.error.errors[field][0];
            userFriendlyErrors.push(`${field}: ${firstErrorMessage}`);
          }
        }
        errorMessage = userFriendlyErrors.join(' | ');
      } else if (error.error.message) {
        errorMessage = error.error.message;
      }
    }
  
    else if (error.status === 401) {
      errorMessage = 'You are not authorized to perform this action. Please log in.';
    }
  
    else if (error.status === 403) {
      errorMessage = 'You do not have permission to perform this action.';
    }
  
    else if (error.status === 404) {
      errorMessage = 'The requested resource was not found.';
    }
  
    // Handle 500 Internal Server Error
    else if (error.status === 500) {
      errorMessage = 'A server error occurred. Please try again later.';
    }
  
    console.error('HTTP Error:', error);
  
    return throwError(() => new Error(errorMessage));
  }
}