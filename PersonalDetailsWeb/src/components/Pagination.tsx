import type { PagedResult } from '../api/types';
import { Icon } from './Icon';

interface PaginationProps {
  result: PagedResult<unknown>;
  onPageChange: (page: number) => void;
}

/** Uses the paging fields computed by the API's PagedResult<T>. */
export function Pagination({ result, onPageChange }: PaginationProps) {
  const { page, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage } = result;
  const first = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const last = Math.min(page * pageSize, totalCount);

  return (
    <nav className="pagination" aria-label="Pages">
      <span className="muted small">
        Showing <strong>{first}–{last}</strong> of <strong>{totalCount}</strong>
      </span>
      <div className="row">
        <button type="button" className="icon-btn bordered" disabled={!hasPreviousPage} onClick={() => onPageChange(page - 1)} aria-label="Previous page">
          <Icon name="back" size={16} />
        </button>
        <span className="small">Page {page} of {Math.max(totalPages, 1)}</span>
        <button type="button" className="icon-btn bordered flip" disabled={!hasNextPage} onClick={() => onPageChange(page + 1)} aria-label="Next page">
          <Icon name="back" size={16} />
        </button>
      </div>
    </nav>
  );
}
