import { useEffect, useRef, useState } from 'react';
import { Download, X } from 'lucide-react';
import { generatePdfBlobUrl, exportFormToPdf } from '../utils/formExport';
import type { Formulario } from '../types';

interface Props {
  form: Formulario | null;
  onClose: () => void;
}

export default function PdfPreviewModal({ form, onClose }: Props) {
  const [blobUrl, setBlobUrl] = useState<string | null>(null);
  const urlRef = useRef<string | null>(null);

  useEffect(() => {
    if (!form) return;
    const url = generatePdfBlobUrl(form);
    urlRef.current = url;
    setBlobUrl(url);
    return () => {
      URL.revokeObjectURL(url);
      urlRef.current = null;
    };
  }, [form]);

  useEffect(() => {
    if (!form) return;
    const handleEscape = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    window.addEventListener('keydown', handleEscape);
    return () => window.removeEventListener('keydown', handleEscape);
  }, [form, onClose]);

  if (!form) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/45 p-4"
      onClick={onClose}
    >
      <div
        className="flex h-[92vh] w-full max-w-4xl flex-col overflow-hidden rounded-2xl bg-white shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex shrink-0 items-center justify-between border-b border-gray-200 px-6 py-4">
          <div>
            <h3 className="font-bold text-gray-900">{form.nome}</h3>
            <p className="text-sm text-gray-500">Pré-visualização do PDF</p>
          </div>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => exportFormToPdf(form)}
              className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-700"
            >
              <Download className="h-4 w-4" />
              Baixar PDF
            </button>
            <button
              type="button"
              onClick={onClose}
              className="rounded-full p-2 text-gray-500 hover:bg-gray-100 hover:text-gray-700"
              aria-label="Fechar"
            >
              <X className="h-5 w-5" />
            </button>
          </div>
        </div>

        <div className="flex-1 bg-gray-100">
          {blobUrl ? (
            <iframe
              src={blobUrl}
              className="h-full w-full"
              title={`PDF - ${form.nome}`}
            />
          ) : (
            <div className="flex h-full items-center justify-center text-sm text-gray-400">
              Gerando PDF...
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
