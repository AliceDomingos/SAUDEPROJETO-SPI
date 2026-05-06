import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FileText, Pencil, Plus } from 'lucide-react';
import { getForms } from '../api';
import type { Formulario } from '../types';
import FormCreateDialog from '../components/FormCreateDialog';
import DataTable, { type Column } from '@/shared/components/table/DataTable';
import { useAuthStore } from '@/shared/store/authStore';
import SearchFiltersPanel from '@/shared/components/filters/SearchFiltersPanel';

export default function FormsListPage() {
  const navigate = useNavigate();
  const canManageForms = useAuthStore((state) => state.canManageForms);
  const [forms, setForms] = useState<Formulario[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('');
  const [showCreateDialog, setShowCreateDialog] = useState(false);

  useEffect(() => {
    loadForms();
  }, []);

  async function loadForms() {
    setLoading(true);
    try {
      const data = await getForms();
      setForms(data);
    } catch {
      // Keep the page usable; existing behavior does not surface this error.
    } finally {
      setLoading(false);
    }
  }

  const filtered = forms.filter((f) =>
    f.nome.toLowerCase().includes(filter.toLowerCase())
  );

  const columns: Column<Formulario>[] = [
    {
      header: 'Acoes',
      render: (f) =>
        canManageForms() ? (
          <button
            type="button"
            onClick={() => navigate(`/formularios/${f.id}`)}
            className="inline-flex items-center gap-1 rounded-lg border border-blue-200 px-3 py-1.5 text-xs font-medium text-blue-700 transition hover:bg-blue-50"
          >
            <Pencil className="h-3.5 w-3.5" />
            Editar
          </button>
        ) : (
          <span className="text-xs text-gray-400">Somente leitura</span>
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
      header: 'Descricao',
      render: (f) => <span className="text-gray-500">{f.descricao || '-'}</span>,
    },
    {
      header: 'Grupo',
      render: (f) => <span className="text-gray-500">{f.groupNome || '-'}</span>,
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
            <h2 className="text-xl font-bold">Formularios</h2>
            <p className="text-sm text-gray-500">{forms.length} formulario(s) cadastrado(s)</p>
          </div>
          {canManageForms() && (
            <button
              onClick={() => setShowCreateDialog(true)}
              className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
            >
              <Plus className="h-4 w-4" />
              Novo Formulario
            </button>
          )}
        </div>

        <SearchFiltersPanel
          title="Encontre formulários com mais rapidez"
          description="Busque pelo nome do formulário."
          searchLabel="Buscar formulário"
          searchValue={filter}
          searchPlaceholder="Buscar por nome..."
          onSearchChange={setFilter}
          hasActiveFilters={filter.trim().length > 0}
          onClear={() => setFilter('')}
        />

        {loading ? (
          <div className="py-8 text-center text-sm text-gray-400">Carregando...</div>
        ) : (
          <DataTable
            data={filtered}
            columns={columns}
            keyExtractor={(f) => f.id}
            emptyMessage={filter ? 'Nenhum formulario encontrado.' : 'Nenhum formulario cadastrado.'}
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
    </>
  );
}
