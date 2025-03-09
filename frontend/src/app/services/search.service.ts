import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  private searchSubject = new Subject<string>();

  searchKeyword$ = this.searchSubject.asObservable();

  updateSearchKeyword(keyword: string): void {
    this.searchSubject.next(keyword);
  }
}