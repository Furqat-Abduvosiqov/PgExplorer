import {Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {Subject} from 'rxjs';
import {debounceTime, distinctUntilChanged, takeUntil} from 'rxjs/operators';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent implements OnInit, OnDestroy {
  @Input() placeholder = 'Search...';
  @Input() debounceTime = 300;
  @Input() showSearchButton = false;
  @Input() disabled = false;
  @Input() initialValue = '';

  @Output() search = new EventEmitter<string>();
  @Output() clear = new EventEmitter<void>();

  searchTerm = '';
  isSearching = false;

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.searchTerm = this.initialValue;

    this.searchSubject
      .pipe(
        debounceTime(this.debounceTime),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(term => {
        this.search.emit(term);
        this.isSearching = false;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchInput(): void {
    if (!this.showSearchButton) {
      this.isSearching = true;
      this.searchSubject.next(this.searchTerm);
    }
  }

  performSearch(): void {
    this.isSearching = true;
    this.search.emit(this.searchTerm);
    this.isSearching = false;
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.search.emit('');
    this.clear.emit();
    this.isSearching = false;
  }
}
