import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ErrorHandlingService } from './error-handling.service';
import { CreateComment } from '../models/comments/createComment';
import { UpdateComment } from '../models/comments/updateComment';

@Injectable({
  providedIn: 'root',
})
export class CommentsDataService {
  private _baseUrl = environment.urlApi.baseUrl;
  private _postUrl = environment.urlApi.postUrl;
  private _subsetUrl = environment.urlApi.subsetUrl;
  private queryPageNumber = environment.urlApi.query.pageNumber;
  private queryKeyword = environment.urlApi.query.keywords;

  constructor(
    private _httpClient: HttpClient,
    private _errorHandlingService: ErrorHandlingService
  ) {}

  getComments(
    postId: string,
    pageNumber: number,
    keyword: string | null
  ): Observable<Comment[]> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}?${this.queryPageNumber}=${pageNumber}`;
    if (keyword) {
      url = `${url}&${this.queryKeyword}=${keyword}`;
    }
    return this._httpClient.get<any>(url).pipe(
      map((response) => (response.data ?? []) as Comment[]),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  getComment(postId: string, commentId: string): Observable<Comment> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}${commentId}`;
    return this._httpClient.get<any>(url).pipe(
      map((response) => response.data as Comment),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  createComment(postId: string, comment: CreateComment): Observable<Comment> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}`;
    return this._httpClient.post<any>(url, comment.toJSON()).pipe(
      map((response) => response.data as Comment),
      catchError((err) => this._errorHandlingService.handleError(err))
    );
  }

  updateComment(
    postId: string,
    commentId: string,
    comment: UpdateComment
  ): Observable<void> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}${commentId}`;
    return this._httpClient
      .put<any>(url, comment.toJSON())
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }

  deleteComment(postId: string, commentId: string): Observable<void> {
    let url: string = `${this._baseUrl}${this._postUrl}${postId}${this._subsetUrl}${commentId}`;
    return this._httpClient
      .delete<void>(url)
      .pipe(catchError((err) => this._errorHandlingService.handleError(err)));
  }
}
