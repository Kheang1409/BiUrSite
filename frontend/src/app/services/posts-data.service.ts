import { HttpClient, HttpErrorResponse } from '@angular/common/http';
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

  private queryTotalPost = environment.urlApi.total;

  constructor(private _httpClient: HttpClient) { }

  getPosts(pageNumber: number, keyword: string | null): Observable<Post[]> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    url = `${url}?${this.queryPageNumber}=${pageNumber}`;
    if (keyword) {
      url = `${url}&${this.queryKeyword}=${keyword}`;
    }
    return this._httpClient.get<Post[]>(url).pipe( // Directly map to the Post[] array
      catchError(this.handleError)
    );
  }

  getPost(postId: number): Observable<Post> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}`;
    return this._httpClient.get<Post>(url).pipe( // Directly map to a single Post
      catchError(this.handleError)
    );
  }

  getTotalPost(): Observable<number> {
    let url: string = `${this._baseUrl}${this._postUrl}${this.queryTotalPost}`;

    return this._httpClient.get<{postCount: number}>(url).pipe( // Directly map to a number
      map(response => response.postCount),
      catchError(this.handleError)
    );
  }

  createPost(newPost: Post): Observable<Post> {
    let url: string = `${this._baseUrl}${this._postUrl}`;
    return this._httpClient.post<Post>(url, newPost.jsonify()).pipe( // Directly map to Post
      catchError(this.handleError)
    );
  }

  updatePost(postId: number, updatedPost: Post): Observable<Post> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}`;
    return this._httpClient.put<Post>(url, updatedPost.jsonify()).pipe( // Directly map to Post
      catchError(this.handleError)
    );
  }

  deletePost(postId: number): Observable<void> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}`;

    return this._httpClient.delete<void>(url).pipe( // No content returned
      catchError(this.handleError)
    );
  }

  getComments(postId: number, pageNumber: number): Observable<Comment[]> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}?${this.queryPageNumber}=${pageNumber}`;
    return this._httpClient.get<Comment[]>(url).pipe(
      catchError(this.handleError)
    );
  }

  createComment(postId: number, newComment: Comment): Observable<Comment> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}`;
    return this._httpClient.post<Comment>(url, newComment.jsonify()).pipe( // Directly map to Comment
      catchError(this.handleError)
    );
  }

  updateComment(postId: number, commentId: number, updateComment: Comment): Observable<Comment> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}${commentId}`;
    return this._httpClient.put<Comment>(url, updateComment.jsonify()).pipe( // Directly map to Comment
      catchError(this.handleError)
    );
  }

  deleteComment(postId: number, commentId: number): Observable<void> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}${commentId}`;
    return this._httpClient.delete<void>(url).pipe( // No content returned
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred.';
    if (error.status === 400) {
      if (error.error.errors) {
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
    } else if (error.status === 401) {
      errorMessage = 'You are not authorized to perform this action. Please log in.';
    } else if (error.status === 403) {
      errorMessage = 'You do not have permission to perform this action.';
    } else if (error.status === 404) {
      errorMessage = 'The requested resource was not found.';
    } else if (error.status === 500) {
      errorMessage = 'A server error occurred. Please try again later.';
    }

    console.error('HTTP Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}