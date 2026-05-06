import { useEffect, useRef, useState } from 'react';
import { Eye, FileText, Filter, Pencil, Plus, Printer, Sheet, Upload, PowerOff, Power } from 'lucide-react';
import { getForms, deactivateForm, activateForm } from '../api';
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
import SearchFiltersPanel from '@/shared/components/filters/SearchFiltersPanel';

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
  const [togglingId, setTogglingId] = useState<number | null>(null);
  const [statusFilter, setStatusFilter] = useState<'todos' | 'ativo' | 'inativo'>('ativo');
  const [statusDropdownOpen, setStatusDropdownOpen] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    loadForms();
  }, []);

  useEffect(() => {
    if (!statusDropdownOpen) return;
    function handleClickOutside() { setStatusDropdownOpen(false); }
    document.addEventListener('click', handleClickOutside);
    return () => document.removeEventListener('click', handleClickOutside);
  }, [statusDropdownOpen]);

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

  async function handleToggleAtivo(f: Formulario) {
    const acao = f.ativo ? 'desativar' : 'reativar';
    if (!window.confirm(`Deseja ${acao} o formulário "${f.nome}"?`)) return;
    setTogglingId(f.id);
    try {
      if (f.ativo) await deactivateForm(f.id);
      else await activateForm(f.id);
      await loadForms();
    } catch {
      // silently fail
    } finally {
      setTogglingId(null);
    }
  }

  const filtered = forms.filter((f) => {
    if (!f.nome.toLowerCase().includes(filter.toLowerCase())) return false;
    if (statusFilter === 'ativo') return f.ativo;
    if (statusFilter === 'inativo') return !f.ativo;
    return true;
  });

  const statusOptions = [
    { value: 'ativo', label: 'Ativo' },
    { value: 'inativo', label: 'Inativo' },
  ] as const;

  const statusHeader = (
    <div className="relative flex items-center gap-1">
      <span>Status</span>
      <button
        type="button"
        onClick={(e) => { e.stopPropagation(); setStatusDropdownOpen((v) => !v); }}
        className={`rounded p-0.5 transition hover:bg-gray-200 ${statusFilter !== 'todos' ? 'text-blue-600' : 'text-gray-400'}`}
      >
        <Filter className="h-3.5 w-3.5" />
      </button>
      {statusDropdownOpen && (
        <div
          className="absolute left-0 top-7 z-30 w-44 rounded-xl border border-gray-200 bg-white shadow-lg"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="flex items-center justify-between border-b border-gray-100 px-3 py-2">
            <button
              type="button"
              onClick={() => { setStatusFilter('todos'); setStatusDropdownOpen(false); }}
              className="text-xs font-medium text-blue-600 hover:underline"
            >
              Todos
            </button>
            <button
              type="button"
              onClick={() => { setStatusFilter('todos'); setStatusDropdownOpen(false); }}
              className="text-xs text-gray-400 hover:text-gray-600"
            >
              Limpar
            </button>
          </div>
          <div className="p-2 space-y-1">
            {statusOptions.map((opt) => (
              <label key={opt.value} className="flex items-center gap-2 rounded px-2 py-1.5 text-sm cursor-pointer hover:bg-gray-50">
                <input
                  type="radio"
                  name="statusFilter"
                  checked={statusFilter === opt.value}
                  onChange={() => { setStatusFilter(opt.value); setStatusDropdownOpen(false); }}
                  className="accent-blue-600"
                />
                {opt.label}
              </label>
            ))}
          </div>
          <div className="border-t border-gray-100 px-3 py-1.5 text-xs text-gray-400">
            {statusFilter === 'todos' ? '0 de 2 selecionados' : '1 de 2 selecionados'}
          </div>
        </div>
      )}
    </div>
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
          {canManageForms() && (
            <button
              type="button"
              title={f.ativo ? 'Desativar' : 'Reativar'}
              onClick={() => handleToggleAtivo(f)}
              disabled={togglingId === f.id}
              className={`rounded-lg border p-2 transition disabled:opacity-50 ${
                f.ativo
                  ? 'border-red-200 text-red-600 hover:bg-red-50'
                  : 'border-gray-300 text-gray-500 hover:bg-gray-50'
              }`}
            >
              {f.ativo ? <PowerOff className="h-3.5 w-3.5" /> : <Power className="h-3.5 w-3.5" />}
            </button>
          )}
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
    {
      header: statusHeader,
      sortKey: (f) => (f.ativo ? 1 : 0),
      render: (f) => (
        <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
          f.ativo ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'
        }`}>
          {f.ativo ? 'Ativo' : 'Inativo'}
        </span>
      ),
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
