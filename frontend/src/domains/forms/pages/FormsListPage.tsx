import { useEffect, useRef, useState } from 'react';
import { Eye, FileText, Pencil, Plus, Printer, Sheet, Upload } from 'lucide-react';
import { getForms } from '../api';
import type { Formulario } from '../types';
import FormCreateDialog from '../components/FormCreateDialog';
import FormDetailsDialog from '../components/FormDetailsDialog';
import FormEditDialog from '../components/FormEditDialog';
import PdfPreviewModal from '../components/PdfPreviewModal';
import DataTable, { type Column } from '@/shared/components/table/DataTable';
import { exportFormToExcel, parseImportedJson, type FormImportData } from '../utils/formExport';
import { parseDocxForm } from '../utils/formDocxImport';
import { parseExcelForm } from '../utils/formExcelImport';
import { useAuthStore } from '@/shared/store/authStore';

export default function FormsListPage() {
  const canManageForms = useAuthStore((state) => state.canManageForms);
  const [forms, setForms] = useState<Formulario[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('');
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [importedData, setImportedData] = useState<FormImportData | undefined>(undefined);
  const [detailsForm, setDetailsForm] = useState<Formulario | null>(null);
  const [editFormId, setEditFormId] = useState<number | null>(null);
  const [importError, setImportError] = useState('');
  const [pdfPreviewForm, setPdfPreviewForm] = useState<Formulario | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    loadForms();
  }, []);

  async function loadForms() {
    setLoading(true);
    try {
      const data = await getForms();
      setForms(data);
    } catch {
      // silently fail
    } finally {
      setLoading(false);
    }
  }

  async function handleImportFile(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    e.target.value = '';
    setImportError('');
    try {
      const name = file.name.toLowerCase();
      const data = name.endsWith('.docx')
        ? await parseDocxForm(file)
        : name.endsWith('.xlsx')
          ? await parseExcelForm(file)
          : await parseImportedJson(file);
      setImportedData(data);
      setShowCreateDialog(true);
    } catch (err) {
      setImportError(err instanceof Error ? err.message : 'Erro ao importar arquivo.');
    }
  }

  const filtered = forms.filter((f) =>
    f.nome.toLowerCase().includes(filter.toLowerCase())
  );

  const columns: Column<Formulario>[] = [
    {
      header: 'Ações',
      sticky: true,
      render: (f) => (
        <div className="flex items-center gap-1.5">
          <button
            type="button"
            title="Visualizar"
            onClick={() => setDetailsForm(f)}
            className="rounded-lg border border-gray-300 p-2 text-gray-700 transition hover:bg-gray-50"
          >
            <Eye className="h-3.5 w-3.5" />
          </button>
          {canManageForms() && (
            <button
              type="button"
              title="Editar"
              onClick={() => setEditFormId(f.id)}
              className="rounded-lg border border-blue-200 p-2 text-blue-700 transition hover:bg-blue-50"
            >
              <Pencil className="h-3.5 w-3.5" />
            </button>
          )}
          <button
            type="button"
            title="Visualizar PDF"
            onClick={() => setPdfPreviewForm(f)}
            className="inline-flex items-center gap-1 rounded-lg border border-gray-200 px-2.5 py-1.5 text-xs font-medium text-gray-600 transition hover:bg-gray-50"
          >
            <Printer className="h-3.5 w-3.5" />
            PDF
          </button>
          <button
            type="button"
            title="Exportar Excel"
            onClick={() => exportFormToExcel(f)}
            className="inline-flex items-center gap-1 rounded-lg border border-green-200 px-2.5 py-1.5 text-xs font-medium text-green-700 transition hover:bg-green-50"
          >
            <Sheet className="h-3.5 w-3.5" />
            Excel
          </button>
        </div>
      ),
    },
    {
      header: 'Nome',
      sortKey: (f) => f.nome,
      render: (f) => (
        <span className="flex items-center gap-2 font-medium text-gray-900">
          <FileText className="h-4 w-4 shrink-0 text-blue-500" />
          {f.nome}
        </span>
      ),
    },
    {
      header: 'Descrição',
      render: (f) => <span className="text-gray-500">{f.descricao || '—'}</span>,
    },
    {
      header: 'Grupo',
      sortKey: (f) => f.groupNome ?? '',
      render: (f) => <span className="text-gray-500">{f.groupNome || '—'}</span>,
    },
    {
      header: 'Perguntas',
      sortKey: (f) => f.perguntas.length,
      render: (f) => <span className="text-gray-600">{f.perguntas.length}</span>,
      className: 'text-center',
    },
    {
      header: 'Peso Total',
      sortKey: (f) => f.pesoTotal,
      render: (f) => <span className="font-medium text-gray-900">{f.pesoTotal}</span>,
      className: 'text-center',
    },
    {
      header: 'Criado por',
      sortKey: (f) => f.criadoPorNome,
      render: (f) => <span className="text-gray-500">{f.criadoPorNome}</span>,
    },
  ];

  return (
    <>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-xl font-bold">Formulários</h2>
            <p className="text-sm text-gray-500">{forms.length} formulário(s) cadastrado(s)</p>
          </div>
          <div className="flex items-center gap-2">
            <input
              ref={fileInputRef}
              type="file"
              accept=".json,.docx,.xlsx"
              className="hidden"
              onChange={handleImportFile}
            />
            <button
              onClick={() => fileInputRef.current?.click()}
              className="flex items-center gap-2 border border-gray-300 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-50 transition text-sm font-medium"
            >
              <Upload className="w-4 h-4" />
              Importar
            </button>
            <button
              onClick={() => setShowCreateDialog(true)}
              className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition text-sm font-medium"
            >
              <Plus className="w-4 h-4" />
              Novo Formulário
            </button>
          </div>
        </div>

        {importError && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
            {importError}
          </div>
        )}

        <input
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          placeholder="Buscar por nome..."
          className="w-full max-w-sm px-3 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500"
        />

        {loading ? (
          <div className="py-8 text-center text-sm text-gray-400">Carregando...</div>
        ) : (
          <DataTable
            data={filtered}
            columns={columns}
            keyExtractor={(f) => f.id}
            emptyMessage="Nenhum formulário cadastrado."
          />
        )}
      </div>

      <FormDetailsDialog
        form={detailsForm}
        open={detailsForm !== null}
        onClose={() => setDetailsForm(null)}
      />

      <FormCreateDialog
        isOpen={showCreateDialog}
        onClose={() => { setShowCreateDialog(false); setImportedData(undefined); }}
        onCreated={() => {
          setShowCreateDialog(false);
          setImportedData(undefined);
          loadForms();
        }}
        initialData={importedData}
      />

      <FormEditDialog
        formId={editFormId}
        onClose={() => setEditFormId(null)}
        onSaved={() => {
          setEditFormId(null);
          loadForms();
        }}
      />

      <PdfPreviewModal
        form={pdfPreviewForm}
        onClose={() => setPdfPreviewForm(null)}
      />
    </>
  );
}
