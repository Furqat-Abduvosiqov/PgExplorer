import {Component, EventEmitter, Input, OnChanges, Output, SimpleChanges} from '@angular/core';
import {CommonModule} from '@angular/common';
import {PageResult} from '../../models/page-result.model';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.scss'
})
export class PaginationComponent implements OnChanges {
  @Input() pageResult: PageResult<any> | null = null;
  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  visiblePages: number[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['pageResult'] && this.pageResult) {
      this.updateVisiblePages();
    }
  }

  goToPage(page: number): void {
    if (this.pageResult && page !== this.pageResult.currentPage && page >= 1 && page <= this.pageResult.totalPages) {
      this.pageChange.emit(page);
    }
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const newPageSize = parseInt(target.value, 10);
    this.pageSizeChange.emit(newPageSize);
  }

  getStartIndex(): number {
    if (!this.pageResult) return 0;
    return (this.pageResult.currentPage - 1) * this.pageResult.pageSize + 1;
  }

  getEndIndex(): number {
    if (!this.pageResult) return 0;
    const endIndex = this.pageResult.currentPage * this.pageResult.pageSize;
    return Math.min(endIndex, this.pageResult.totalCount);
  }

  getVisiblePages(): number[] {
    return this.visiblePages;
  }

  private updateVisiblePages(): void {
    if (!this.pageResult) {
      this.visiblePages = [];
      return;
    }

    const current = this.pageResult.currentPage;
    const total = this.pageResult.totalPages;
    const delta = 2;

    let pages: number[] = [];

    // Always show first page
    if (total > 0) {
      pages.push(1);
    }

    // Add pages around current page
    for (let i = Math.max(2, current - delta); i <= Math.min(total - 1, current + delta); i++) {
      pages.push(i);
    }

    // Always show last page
    if (total > 1) {
      pages.push(total);
    }

    // Remove duplicates and sort
    this.visiblePages = [...new Set(pages)].sort((a, b) => a - b);
  }

}
