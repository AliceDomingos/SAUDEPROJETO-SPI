import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { getEvals } from '@/domains/dashboard/api';
import type { Evaluation } from '@/types';
import { Eye, Plus } from 'lucide-react';
import EvaluationCreateDialog from '../components/EvaluationCreateDialog';
import DataTable, { type Column } from '@/shared/components/table/DataTable';
import { useAuthStore } from '@/shared/store/authStore';
import SearchFiltersPanel from '@/shared/components/filters/SearchFiltersPanel';

export default function EvaluationsListPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const canCreateEvaluations = useAuthStore((state) => state.canCreateEvaluations);
  const [evals, setEvals] = useState<Evaluation[]>([]);
  const [filter, setFilter] = useState('');
  const [createOpen, setCreateOpen] = useState(false);

  useEffect(() => {
    getEvals().then((data: Evaluation[]) => setEvals(data));
  }, []);

  useEffect(() => {
    const state = location.state as { openNewEvaluation?: boolean } | null;
    if (state?.openNewEvaluation && canCreateEvaluations()) {
      setCreateOpen(true);
      navigate(location.pathname, { replace: true, state: null });
    }
  }, [canCreateEvaluations, location.pathname, location.state, navigate]);

  const filtered = evals.filter((e) =>
    e.patientNome.toLowerCase().includes(filter.toLowerCase())
  );

  const badgeCls = (score: number) => {
    if (score <= 29.5) return 'bg-green-100 text-green-700';
    if (score < 37) return 'bg-amber-100 text-amber-700';
    return 'bg-red-100 text-red-700';
  };

  const columns: Column<Evaluation>[] = [
    {
      header: 'Ações',
      render: (e) => (
        <button
          type="button"
          onClick={() => navigate(`/avaliacoes/${e.id}`)}
          className="inline-flex items-center gap-1 rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 transition hover:bg-gray-50"
        >
          <Eye className="h-3.5 w-3.5" />
          Visualizar
        </button>
      ),
    },
    {
      header: 'Paciente',
      render: (e) => <span className="font-medium text-gray-900">{e.patientNome}</span>,
    },
    {
      header: 'Avaliador',
      render: (e) => <span className="text-gray-500">{e.avaliadorNome}</span>,
    },
    {
      header: 'Data',
      render: (e) => <span className="text-gray-500">{new Date(e.dataAvaliacao).toLocaleDateString('pt-BR')}</span>,
    },
    {
      header: 'Score',
      render: (e) => (
        <span className={`rounded-full px-2 py-1 text-xs font-bold ${badgeCls(e.scoreTotal)}`}>
          {e.scoreTotal}/60
        </span>
      ),
    },
    {
      header: 'Encaminhamento',
      render: (e) => {
        if (!e.referral) return <span className="text-xs text-gray-400">Pendente</span>;
        return e.referral.encaminhado ? (
          <span className="rounded-full bg-blue-100 px-2 py-1 text-xs font-bold text-blue-700">Encaminhado</span>
        ) : (
          <span className="rounded-full bg-gray-100 px-2 py-1 text-xs font-bold text-gray-600">Não encaminhado</span>
        );
      },
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold">Avaliacoes</h2>
          <p className="text-sm text-gray-500">Historico completo</p>
        </div>
        {canCreateEvaluations() && (
          <button
            onClick={() => setCreateOpen(true)}
            className="flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700"
          >
            <Plus className="h-4 w-4" />
            Nova Avaliacao
          </button>
        )}
      </div>

      <SearchFiltersPanel
        title="Encontre avaliações com mais rapidez"
        description="Busque pelo nome do paciente ou avaliador."
        searchLabel="Buscar avaliação"
        searchValue={filter}
        searchPlaceholder="Buscar por paciente..."
        onSearchChange={setFilter}
        hasActiveFilters={filter.trim().length > 0}
        onClear={() => setFilter('')}
      />

      <DataTable
        data={filtered}
        columns={columns}
        keyExtractor={(e) => e.id}
        emptyMessage="Nenhuma avaliação encontrada."
      />

      {canCreateEvaluations() && <EvaluationCreateDialog open={createOpen} onClose={() => setCreateOpen(false)} />}
    </div>
  );
}
