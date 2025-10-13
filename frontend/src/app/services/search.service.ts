import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class SearchService {
  private searchSubject = new Subject<string>();

  public searchKeyword$ = this.searchSubject.pipe(
    debounceTime(250),
    distinctUntilChanged()
  );

  updateSearchKeyword(keyword: string): void {
    this.searchSubject.next(keyword ?? '');
  }
}
