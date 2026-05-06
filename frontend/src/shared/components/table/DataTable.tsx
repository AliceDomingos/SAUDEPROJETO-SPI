import { useState } from 'react';
import type { ReactNode } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';

export interface Column<T> {
  header: string;
  render: (row: T) => ReactNode;
  className?: string;
  align?: 'left' | 'right';
}

interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  keyExtractor: (row: T) => string | number;
  emptyMessage?: string;
  rowClassName?: string;
  pageSize?: number;
}

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

export default function DataTable<T>({
  data,
  columns,
  keyExtractor,
  emptyMessage = 'Nenhum registro encontrado.',
  rowClassName = '',
  pageSize: initialPageSize = 10,
}: DataTableProps<T>) {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(initialPageSize);

  const totalPages = Math.max(1, Math.ceil(data.length / pageSize));
  const safePage = Math.min(page, totalPages);
  const start = (safePage - 1) * pageSize;
  const pageData = data.slice(start, start + pageSize);

  function handlePageSizeChange(size: number) {
    setPageSize(size);
    setPage(1);
  }

  return (
    <div className="space-y-3">
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <div className="overflow-x-auto overflow-y-auto" style={{ maxHeight: '480px' }}>
          <table className="w-full text-sm">
            <thead className="border-b border-gray-200 bg-gray-50">
              <tr>
                {columns.map((col, i) => (
                  <th
                    key={i}
                    className={`sticky top-0 z-10 bg-gray-50 px-4 py-3 font-medium text-gray-600 ${col.align === 'right' ? 'text-right' : 'text-left'} ${col.className ?? ''}`}
                  >
                    {col.header}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data.length === 0 ? (
                <tr>
                  <td colSpan={columns.length} className="px-4 py-10 text-center text-sm text-gray-500">
                    {emptyMessage}
                  </td>
                </tr>
              ) : (
                pageData.map((row) => (
                  <tr
                    key={keyExtractor(row)}
                    className={`border-b border-gray-100 hover:bg-gray-50 ${rowClassName}`}
                  >
                    {columns.map((col, i) => (
                      <td
                        key={i}
                        className={`px-4 py-3 ${col.align === 'right' ? 'text-right' : ''} ${col.className ?? ''}`}
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
      </div>

      {data.length > 0 && (
        <div className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-gray-200 bg-white px-4 py-3">
          {/* Seletor de tamanho */}
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <span>Exibir</span>
            <div className="flex gap-1">
              {PAGE_SIZE_OPTIONS.map((size) => (
                <button
                  key={size}
                  type="button"
                  onClick={() => handlePageSizeChange(size)}
                  className={`rounded-lg px-3 py-1.5 text-xs font-medium transition ${
                    pageSize === size
                      ? 'bg-blue-600 text-white'
                      : 'border border-gray-200 text-gray-600 hover:bg-gray-50'
                  }`}
                >
                  {size}
                </button>
              ))}
            </div>
            <span>por página</span>
          </div>

          {/* Info + navegação */}
          <div className="flex items-center gap-4">
            <span className="text-xs text-gray-500">
              <span className="font-semibold text-gray-700">{start + 1}–{Math.min(start + pageSize, data.length)}</span>
              {' '}de{' '}
              <span className="font-semibold text-gray-700">{data.length}</span>
              {' '}registros
            </span>

            <div className="flex items-center gap-1">
              <button
                type="button"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={safePage === 1}
                className="inline-flex items-center gap-1 rounded-lg border border-gray-200 px-2.5 py-1.5 text-xs font-medium text-gray-600 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-40"
              >
                <ChevronLeft className="h-3.5 w-3.5" />
                Anterior
              </button>

              <span className="min-w-[64px] text-center text-xs font-medium text-gray-700">
                {safePage} / {totalPages}
              </span>

              <button
                type="button"
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={safePage === totalPages}
                className="inline-flex items-center gap-1 rounded-lg border border-gray-200 px-2.5 py-1.5 text-xs font-medium text-gray-600 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-40"
              >
                Próximo
                <ChevronRight className="h-3.5 w-3.5" />
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
