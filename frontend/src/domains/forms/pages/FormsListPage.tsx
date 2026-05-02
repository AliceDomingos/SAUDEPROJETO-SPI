import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FileText, Pencil, Plus, Printer, Upload } from 'lucide-react';
import { getForms } from '../api';
import type { Formulario } from '../types';
import FormCreateDialog from '../components/FormCreateDialog';
import PdfPreviewModal from '../components/PdfPreviewModal';
import DataTable, { type Column } from '@/shared/components/table/DataTable';
import { parseImportedJson } from '../utils/formExport';
import { parseDocxForm } from '../utils/formDocxImport';

export default function FormsListPage() {
  const navigate = useNavigate();
  const [forms, setForms] = useState<Formulario[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('');
  const [showCreateDialog, setShowCreateDialog] = useState(false);
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
      const isDocx = file.name.toLowerCase().endsWith('.docx');
      const data = isDocx ? await parseDocxForm(file) : await parseImportedJson(file);
      navigate('/formularios/novo', { state: data });
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
      render: (f) => (
        <div className="flex items-center gap-1.5">
          <button
            type="button"
            onClick={() => navigate(`/formularios/${f.id}`)}
            className="inline-flex items-center gap-1 rounded-lg border border-blue-200 px-3 py-1.5 text-xs font-medium text-blue-700 transition hover:bg-blue-50"
          >
            <Pencil className="h-3.5 w-3.5" />
            Editar
          </button>
          <button
            type="button"
            onClick={() => setPdfPreviewForm(f)}
            title="Visualizar PDF"
            className="inline-flex items-center gap-1 rounded-lg border border-gray-200 px-2.5 py-1.5 text-xs font-medium text-gray-600 transition hover:bg-gray-50"
          >
            <Printer className="h-3.5 w-3.5" />
            PDF
          </button>
        </div>
      ),
    },
    {
      header: 'Nome',
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
      render: (f) => <span className="text-gray-500">{f.groupNome || '—'}</span>,
    },
    {
      header: 'Perguntas',
      render: (f) => <span className="text-gray-600">{f.perguntas.length}</span>,
      className: 'text-center',
    },
    {
      header: 'Peso Total',
      render: (f) => <span className="font-medium text-gray-900">{f.pesoTotal}</span>,
      className: 'text-center',
    },
    {
      header: 'Criado por',
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
              accept=".json,.docx"
              className="hidden"
              onChange={handleImportFile}
            />
            <button
              onClick={() => fileInputRef.current?.click()}
              className="flex items-center gap-2 border border-gray-300 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-50 transition text-sm font-medium"
            >
              <Upload className="w-4 h-4" />
              Importar JSON
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

      <FormCreateDialog
        isOpen={showCreateDialog}
        onClose={() => setShowCreateDialog(false)}
        onCreated={() => {
          setShowCreateDialog(false);
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
