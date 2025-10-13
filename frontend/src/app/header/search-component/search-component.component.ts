import { Component } from '@angular/core';
import { SearchService } from '../../services/search.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-component.component.html',
  styleUrls: ['./search-component.component.css'],
})
export class SearchComponent {
  searchKeyword: string = '';
  mobileSearchVisible: boolean = false;

  constructor(private searchService: SearchService) {
    // this.searchKeyword = sessionStorage.getItem('searchKeywords') || '';
  }

  onSearch(): void {
    const keyword = this.searchKeyword.trim();
    this.searchService.updateSearchKeyword(keyword);
    // sessionStorage.setItem('searchKeywords', keyword);
  }

  toggleMobileSearch(event: Event): void {
    event.stopPropagation();
    this.mobileSearchVisible = !this.mobileSearchVisible;
    if (!this.mobileSearchVisible) this.onSearch();
  }

  closeMobileSearch() {
    this.mobileSearchVisible = false;
  }
}
