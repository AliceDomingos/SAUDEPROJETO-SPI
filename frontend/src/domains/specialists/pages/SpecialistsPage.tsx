import { useEffect, useState } from 'react';
import { Pencil, Plus, UserX } from 'lucide-react';
import SearchFiltersPanel from '@/shared/components/filters/SearchFiltersPanel';
import DataTable, { type Column } from '@/shared/components/table/DataTable';
import { useAuthStore } from '@/shared/store/authStore';
import {
  createSpecialist,
  deactivateSpecialist,
  getSpecialists,
  updateSpecialist,
  type CreateSpecialistInput,
  type UpdateSpecialistInput,
} from '../api';
import type { Specialist } from '../types';
import SpecialistCreateDialog from '../components/dialogs/SpecialistCreateDialog';
import SpecialistEditDialog from '../components/dialogs/SpecialistEditDialog';
import SpecialistDeactivateDialog from '../components/dialogs/SpecialistDeactivateDialog';
import {
  formatCurrency,
  formatSpecialistDate,
  getSpecialistSearchText,
} from '../components/utils/specialistUtils';

export default function SpecialistsPage() {
  const canManageSpecialists = useAuthStore((state) => state.canManageSpecialists);
  const [specialists, setSpecialists] = useState<Specialist[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [editSpecialist, setEditSpecialist] = useState<Specialist | null>(null);
  const [deactivateTarget, setDeactivateTarget] = useState<Specialist | null>(null);

  const loadData = async () => {
    const data = await getSpecialists();
    setSpecialists(data);
    setError('');
  };

  useEffect(() => {
    const initialize = async () => {
      try {
        await loadData();
      } catch (err: unknown) {
        setError(err instanceof Error ? err.message : 'Erro ao carregar especialistas');
      } finally {
        setLoading(false);
      }
    };

    initialize();
  }, []);

  const handleCreate = async (data: CreateSpecialistInput) => {
    await createSpecialist(data);
    await loadData();
  };

  const handleEdit = async (specialistId: string, data: UpdateSpecialistInput) => {
    await updateSpecialist(specialistId, data);
    await loadData();
  };

  const handleDeactivate = async (specialistId: string) => {
    await deactivateSpecialist(specialistId);
    await loadData();
  };

  const normalizedSearch = search.trim().toLowerCase();
  const filteredSpecialists = specialists.filter((specialist) => {
    if (!normalizedSearch) return true;
    return getSpecialistSearchText(specialist).includes(normalizedSearch);
  });

  const columns: Column<Specialist>[] = [
    {
      header: 'Acoes',
      render: (specialist) => (
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => setEditSpecialist(specialist)}
            disabled={!canManageSpecialists()}
            className="inline-flex items-center gap-1 rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 transition hover:bg-gray-50"
          >
            <Pencil className="h-3.5 w-3.5" />
            Editar
          </button>
          {canManageSpecialists() && specialist.ativo && (
            <button
              type="button"
              onClick={() => setDeactivateTarget(specialist)}
              className="inline-flex items-center gap-1 rounded-lg border border-red-200 px-3 py-1.5 text-xs font-medium text-red-700 transition hover:bg-red-50"
            >
              <UserX className="h-3.5 w-3.5" />
              Desativar
            </button>
          )}
        </div>
      ),
    },
    {
      header: 'Especialista',
      render: (specialist) => (
        <>
          <div className="font-medium text-gray-900">{specialist.nome}</div>
          <div className="text-xs text-gray-500">{specialist.especialidade}</div>
        </>
      ),
    },
    {
      header: 'Valor',
      render: (specialist) => <span className="font-semibold text-gray-700">{formatCurrency(specialist.custoConsulta)}</span>,
    },
    {
      header: 'Status',
      render: (specialist) => (
        <span className={`rounded-full px-2 py-1 text-xs font-bold ${specialist.ativo ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-600'}`}>
          {specialist.ativo ? 'Ativo' : 'Inativo'}
        </span>
      ),
    },
    {
      header: 'Cadastro',
      render: (specialist) => <span className="text-gray-500">{formatSpecialistDate(specialist.criadoEm)}</span>,
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <h2 className="text-xl font-bold">Especialistas</h2>
          <p className="text-sm text-gray-500">
            {filteredSpecialists.length} especialista(s) exibido(s) de {specialists.length} cadastrado(s)
          </p>
        </div>

        {canManageSpecialists() && (
          <button
            type="button"
            onClick={() => setCreateOpen(true)}
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
          >
            <Plus className="h-4 w-4" />
            Novo especialista
          </button>
        )}
      </div>

      <SearchFiltersPanel
        title="Encontre especialistas por nome ou area"
        description="Busque pelo profissional ou pela especialidade antes de encaminhar um paciente."
        searchLabel="Buscar especialista"
        searchValue={search}
        searchPlaceholder="Buscar por nome ou especialidade"
        onSearchChange={setSearch}
        hasActiveFilters={search.trim().length > 0}
        onClear={() => setSearch('')}
      />

      {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

      {loading ? (
        <div className="rounded-xl border border-gray-200 bg-white px-6 py-10 text-center text-sm text-gray-500">
          Carregando especialistas...
        </div>
      ) : (
        <DataTable
          data={filteredSpecialists}
          columns={columns}
          keyExtractor={(specialist) => specialist.id}
          emptyMessage="Nenhum especialista encontrado com os filtros atuais."
          rowClassName="align-top"
        />
      )}

      <SpecialistCreateDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={handleCreate} />
      <SpecialistEditDialog
        specialist={editSpecialist}
        open={editSpecialist !== null}
        onClose={() => setEditSpecialist(null)}
        onSubmit={handleEdit}
      />
      <SpecialistDeactivateDialog
        specialist={deactivateTarget}
        open={deactivateTarget !== null}
        onClose={() => setDeactivateTarget(null)}
        onConfirm={handleDeactivate}
      />
    </div>
  );
}
