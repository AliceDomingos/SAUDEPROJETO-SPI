import { useState } from 'react';
import type { ReactNode } from 'react';
import { ChevronDown, ChevronLeft, ChevronRight, ChevronUp, ChevronsUpDown } from 'lucide-react';


export interface Column<T> {
  header: ReactNode;
  render: (row: T) => ReactNode;
  className?: string;
  align?: 'left' | 'right';
  sortKey?: (row: T) => string | number;
  sticky?: boolean;
}

interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  keyExtractor: (row: T) => string | number;
  emptyMessage?: string;
  rowClassName?: string;
  pageSize?: number;
}

export default function DataTable<T>({
  data,
  columns,
  keyExtractor,
  emptyMessage = 'Nenhum registro encontrado.',
  rowClassName = '',
  pageSize = 10,
}: DataTableProps<T>) {
  const [sortCol, setSortCol] = useState<number | null>(null);
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');
  const [page, setPage] = useState(1);

  let sorted = data;
  if (sortCol !== null) {
    const key = columns[sortCol]?.sortKey;
    if (key) {
      sorted = [...data].sort((a, b) => {
        const av = key(a);
        const bv = key(b);
        if (av < bv) return sortDir === 'asc' ? -1 : 1;
        if (av > bv) return sortDir === 'asc' ? 1 : -1;
        return 0;
      });
    }
  }

  const totalPages = Math.max(1, Math.ceil(sorted.length / pageSize));
  const safePage = Math.min(page, totalPages);
  const start = (safePage - 1) * pageSize;
  const paginated = sorted.slice(start, start + pageSize);

  function handleSort(colIndex: number) {
    if (colIndex === sortCol) {
      setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortCol(colIndex);
      setSortDir('asc');
    }
    setPage(1);
  }

  function pageNumbers(): (number | '…')[] {
    const result: (number | '…')[] = [];
    let prev: number | null = null;
    for (let i = 1; i <= totalPages; i++) {
      if (i === 1 || i === totalPages || Math.abs(i - safePage) <= 1) {
        if (prev !== null && i - prev > 1) result.push('…');
        result.push(i);
        prev = i;
      }
    }
    return result;
  }

  return (
    <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="border-b border-gray-200 bg-gray-50">
            <tr>
              {columns.map((col, i) => (
                <th
                  key={i}
                  onClick={col.sortKey ? () => handleSort(i) : undefined}
                  className={[
                    'px-4 py-3 font-medium text-gray-600',
                    col.align === 'right' ? 'text-right' : 'text-left',
                    col.className ?? '',
                    col.sortKey ? 'cursor-pointer select-none hover:bg-gray-100' : '',
                    col.sticky ? 'sticky left-0 z-10 bg-gray-50 border-r border-gray-200' : '',
                  ].filter(Boolean).join(' ')}
                >
                  {col.sortKey ? (
                    <div className="flex items-center gap-1">
                      {col.header}
                      {sortCol === i
                        ? sortDir === 'asc'
                          ? <ChevronUp className="h-3.5 w-3.5 text-blue-600" />
                          : <ChevronDown className="h-3.5 w-3.5 text-blue-600" />
                        : <ChevronsUpDown className="h-3.5 w-3.5 text-gray-400" />}
                    </div>
                  ) : (
                    col.header
                  )}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {paginated.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="px-4 py-10 text-center text-sm text-gray-500">
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              paginated.map((row) => (
                <tr
                  key={keyExtractor(row)}
                  className={`group border-b border-gray-100 hover:bg-gray-50 ${rowClassName}`}
                >
                  {columns.map((col, i) => (
                    <td
                      key={i}
                      className={[
                        'px-4 py-3',
                        col.align === 'right' ? 'text-right' : '',
                        col.className ?? '',
                        col.sticky
                          ? 'sticky left-0 z-10 bg-white group-hover:bg-gray-50 border-r border-gray-200'
                          : '',
                      ].filter(Boolean).join(' ')}
                    >
                      {col.render(row)}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between border-t border-gray-100 px-4 py-3 text-xs text-gray-500">
        <span>
          {sorted.length === 0
            ? 'Nenhum registro'
            : `${start + 1}–${Math.min(start + pageSize, sorted.length)} de ${sorted.length}`}
        </span>
        {totalPages > 1 && (
          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={safePage === 1}
              className="rounded p-1.5 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-40"
            >
              <ChevronLeft className="h-4 w-4" />
            </button>
            {pageNumbers().map((p, i) =>
              p === '…' ? (
                <span key={`e${i}`} className="px-1">…</span>
              ) : (
                <button
                  key={p}
                  type="button"
                  onClick={() => setPage(p as number)}
                  className={`min-w-[2rem] rounded px-2 py-1 font-medium transition ${
                    safePage === p ? 'bg-blue-600 text-white' : 'hover:bg-gray-100'
                  }`}
                >
                  {p}
                </button>
              ),
            )}
            <button
              type="button"
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={safePage === totalPages}
              className="rounded p-1.5 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-40"
            >
              <ChevronRight className="h-4 w-4" />
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
