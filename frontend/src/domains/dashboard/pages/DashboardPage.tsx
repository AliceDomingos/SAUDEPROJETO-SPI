import { useEffect, useMemo, useState } from 'react';
import { Activity, Banknote, CalendarDays, CheckCircle2, ClipboardList, Filter, Route, Sparkles, TrendingUp, X } from 'lucide-react';
import { getDashboardGroups, getDashboardStats, type DashboardFilter, type SpiMockSusDashboard } from '../api';
import { DistributionDonut, HorizontalBars, MonthlyBars } from '../components/DashboardCharts';
import PeriodFilter, { type PeriodSelection } from '../components/PeriodFilter';
import StatsCards from '../components/StatsCards';
import type { Group } from '@/domains/groups/types';
import { useAuthStore } from '@/shared/store/authStore';

type DashboardFilterState = {
  risco?: string;
  especialista?: string;
  dataInicio?: string;
  dataFim?: string;
  periodoLabel?: string;
  monthLabel?: string;
  grupoId?: string;
  grupoNome?: string;
};

function formatDate(date: string | null | undefined) {
  return date ? new Date(date).toLocaleDateString('pt-BR') : '-';
}

function formatPercent(value: number) {
  return `${value.toFixed(1)}%`;
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 }).format(value);
}

function formatDateInput(date: string) {
  return date ? new Date(`${date}T00:00:00`).toLocaleDateString('pt-BR') : '-';
}

function toDateInputValue(date: string | null | undefined) {
  return date ? new Date(date).toISOString().slice(0, 10) : '';
}

function hasFilters(filters: DashboardFilterState) {
  return Boolean(filters.risco || filters.especialista || filters.dataInicio || filters.dataFim || filters.grupoId);
}

function toApiFilter(filters: DashboardFilterState): DashboardFilter {
  return {
    risco: filters.risco,
    especialista: filters.especialista,
    dataInicio: filters.dataInicio,
    dataFim: filters.dataFim,
    grupoId: filters.grupoId,
  };
}

function getMonthRangeFromLabel(label: string) {
  const [rawMonth, rawYear] = label.split('/');
  const monthMap: Record<string, number> = {
    jan: 0,
    fev: 1,
    mar: 2,
    abr: 3,
    mai: 4,
    jun: 5,
    jul: 6,
    ago: 7,
    set: 8,
    out: 9,
    nov: 10,
    dez: 11,
  };
  const month = monthMap[(rawMonth ?? '').toLocaleLowerCase('pt-BR').slice(0, 3)];
  const year = 2000 + Number(rawYear);

  if (month === undefined || Number.isNaN(year)) return null;

  const start = new Date(year, month, 1);
  const end = new Date(year, month + 1, 0);

  return {
    start: start.toISOString().slice(0, 10),
    end: end.toISOString().slice(0, 10),
  };
}

export default function DashboardPage() {
  const [baseStats, setBaseStats] = useState<SpiMockSusDashboard | null>(null);
  const [filteredStats, setFilteredStats] = useState<SpiMockSusDashboard | null>(null);
  const [filters, setFilters] = useState<DashboardFilterState>({});
  const [groups, setGroups] = useState<Group[]>([]);
  const [error, setError] = useState<string | null>(null);
  const isAnalyst = useAuthStore((state) => state.user?.role === 'analista');

  const filtersActive = hasFilters(filters);
  const stats = filtersActive ? filteredStats ?? baseStats : baseStats;

  useEffect(() => {
    getDashboardStats()
      .then((data) => {
        setBaseStats(data);
      })
      .catch((err) => {
        console.error('Erro ao carregar dashboard:', err);
        setError('Erro ao carregar indicadores do CSV');
      });
  }, []);

  useEffect(() => {
    if (!isAnalyst) return;

    let isCurrent = true;
    getDashboardGroups()
      .then((data) => {
        if (isCurrent) setGroups(data);
      })
      .catch((err) => {
        console.error('Erro ao carregar grupos do dashboard:', err);
      });

    return () => {
      isCurrent = false;
    };
  }, [isAnalyst]);

  useEffect(() => {
    if (!baseStats || !filtersActive) return;

    let isCurrent = true;
    getDashboardStats(toApiFilter(filters))
      .then((data) => {
        if (isCurrent) setFilteredStats(data);
      })
      .catch((err) => {
        console.error('Erro ao filtrar dashboard:', err);
        if (isCurrent) setError('Erro ao filtrar indicadores do CSV');
      });

    return () => {
      isCurrent = false;
    };
  }, [baseStats, filters, filtersActive]);

  const activeChips = useMemo(
    () => [
      filters.periodoLabel ? { key: 'periodo', label: `Período: ${filters.periodoLabel}` } : null,
      filters.grupoId ? { key: 'grupo', label: `Grupo: ${filters.grupoNome ?? groups.find((group) => group.id === filters.grupoId)?.nome ?? filters.grupoId}` } : null,
      filters.risco ? { key: 'risco', label: `Risco: ${filters.risco}` } : null,
      filters.especialista ? { key: 'especialista', label: `Especialidade: ${filters.especialista}` } : null,
    ].filter((item): item is { key: string; label: string } => Boolean(item)),
    [filters, groups]
  );

  if (error && !stats) return <div className="text-center py-8 text-red-400">{error}</div>;
  if (!stats) return <div className="text-center py-8 text-gray-400">Carregando...</div>;

  const fullPeriodStart = toDateInputValue(baseStats?.periodoTriagens.inicio ?? stats.periodoTriagens.inicio);
  const fullPeriodEnd = toDateInputValue(baseStats?.periodoTriagens.fim ?? stats.periodoTriagens.fim);
  const displayedPeriod = filters.periodoLabel ?? `${formatDate(stats.periodoTriagens.inicio)} - ${formatDate(stats.periodoTriagens.fim)}`;
  const monthlyData = filters.monthLabel ? baseStats?.distribuicaoTriagensMensais ?? stats.distribuicaoTriagensMensais : stats.distribuicaoTriagensMensais;
  const riskData = filters.risco ? baseStats?.distribuicaoRisco ?? stats.distribuicaoRisco : stats.distribuicaoRisco;
  const specialistData = filters.especialista ? baseStats?.distribuicaoEspecialista ?? stats.distribuicaoEspecialista : stats.distribuicaoEspecialista;

  const clearFilter = (key: string) => {
    setFilters((current) => {
      if (key === 'periodo') {
        const { dataInicio, dataFim, periodoLabel, monthLabel, ...rest } = current;
        void dataInicio;
        void dataFim;
        void periodoLabel;
        void monthLabel;
        return rest;
      }
      if (key === 'risco') {
        const { risco, ...rest } = current;
        void risco;
        return rest;
      }
      if (key === 'especialista') {
        const { especialista, ...rest } = current;
        void especialista;
        return rest;
      }
      if (key === 'grupo') {
        const { grupoId, grupoNome, ...rest } = current;
        void grupoId;
        void grupoNome;
        return rest;
      }
      return current;
    });
  };

  const applyGroupFilter = (groupId: string) => {
    setFilters((current) => {
      if (!groupId) {
        const { grupoId: _grupoId, grupoNome, ...rest } = current;
        void _grupoId;
        void grupoNome;
        return rest;
      }

      const group = groups.find((item) => item.id === groupId);
      return {
        ...current,
        grupoId: groupId,
        grupoNome: group?.nome,
      };
    });
  };

  const applyPeriod = (selection: PeriodSelection) => {
    setFilters((current) => ({
      ...current,
      dataInicio: selection.dataInicio,
      dataFim: selection.dataFim,
      periodoLabel: selection.label,
      monthLabel: selection.monthLabel,
    }));
  };

  const applyMonthlyFilter = (label: string) => {
    const range = getMonthRangeFromLabel(label);
    if (!range) return;

    setFilters((current) => {
      if (current.monthLabel === label) {
        const { dataInicio, dataFim, periodoLabel, monthLabel, ...rest } = current;
        void dataInicio;
        void dataFim;
        void periodoLabel;
        void monthLabel;
        return rest;
      }

      return {
        ...current,
        dataInicio: range.start,
        dataFim: range.end,
        periodoLabel: `${formatDateInput(range.start)} - ${formatDateInput(range.end)}`,
        monthLabel: label,
      };
    });
  };

  return (
    <div className="space-y-7">
      <header className="rounded-2xl border border-blue-100 bg-gradient-to-r from-blue-50 via-white to-cyan-50 p-5 shadow-sm">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
          <div>
            <div className="flex items-center gap-2 text-sm font-bold text-blue-700">
              <Sparkles className="h-4 w-4" />
              NEXOS · Dashboard Triagem
            </div>
            <h2 className="mt-1 text-2xl font-extrabold tracking-tight text-gray-900">Indicadores gerenciais SPI</h2>
            <p className="mt-1 text-sm text-gray-500">Base integrada via API: {stats.fonte}</p>

            {activeChips.length ? (
              <div className="mt-3 flex flex-wrap gap-2">
                {activeChips.map((chip) => (
                  <button
                    key={chip.key}
                    type="button"
                    onClick={() => clearFilter(chip.key)}
                    className="inline-flex max-w-full items-center gap-2 rounded-lg border border-blue-100 bg-white/80 px-3 py-2 text-sm font-extrabold text-gray-900 shadow-sm transition hover:bg-blue-50"
                    title="Remover filtro"
                  >
                    <Filter className="h-4 w-4 shrink-0 text-blue-700" />
                    <span className="truncate">{chip.label}</span>
                    <X className="h-4 w-4 shrink-0 text-blue-700" />
                  </button>
                ))}
              </div>
            ) : null}
          </div>

          <div className={`grid grid-cols-2 gap-2 ${isAnalyst ? 'md:grid-cols-5' : 'md:grid-cols-4'}`}>
            <PeriodFilter value={displayedPeriod} fullStart={fullPeriodStart} fullEnd={fullPeriodEnd} onApply={applyPeriod} onClear={() => clearFilter('periodo')} />
            {isAnalyst ? <GroupFilter value={filters.grupoId} groups={groups} onChange={applyGroupFilter} /> : null}
            <Metric label="Base" value={`${stats.totalTriagens.toLocaleString('pt-BR')} registros`} />
            <Metric label="Score" value={`${stats.menorScore}-${stats.maiorScore}`} />
            <Metric label="Atualização" value={formatDate(stats.atualizadoEm)} />
          </div>
        </div>
      </header>

      <SectionTitle title="Visão Geral" />
      <StatsCards
        items={[
          { icon: ClipboardList, label: 'Crianças triadas', value: stats.totalTriagens.toLocaleString('pt-BR'), tone: 'blue', tag: 'KPI 01', foot: filtersActive ? 'recorte selecionado no dashboard' : 'base completa do CSV' },
          { icon: TrendingUp, label: 'Score médio', value: stats.scoreMedio.toFixed(1), tone: 'amber', tag: 'KPI 02', foot: 'CARS-2 / M-CHAT-R/F' },
          { icon: Activity, label: 'Casos severos', value: stats.casosSeveros, tone: 'red', tag: 'KPI 03', foot: `${formatPercent((stats.casosSeveros * 100) / Math.max(stats.totalTriagens, 1))} das triagens` },
          { icon: CalendarDays, label: 'Triagens no mês', value: stats.triagensMesAtual, tone: 'green', tag: 'KPI 04', foot: 'último mês da base' },
        ]}
      />

      <div className="grid grid-cols-1 gap-5 xl:grid-cols-[1.45fr_1fr]">
        <ChartCard title="Crianças triadas por mês" subtitle="Clique em uma coluna para filtrar pelo mês">
          <MonthlyBars data={monthlyData.slice(-16)} selectedLabel={filters.monthLabel} onSelect={applyMonthlyFilter} />
        </ChartCard>

        <ChartCard title="Distribuição por nível de risco" subtitle="Clique em um nível para filtrar o dashboard">
          <DistributionDonut
            data={riskData}
            selectedLabel={filters.risco}
            onSelect={(label) => setFilters((current) => ({ ...current, risco: current.risco === label ? undefined : label }))}
          />
        </ChartCard>
      </div>

      <SectionTitle title="Encaminhamentos" tone="purple" />
      <StatsCards
        columnsClassName="grid-cols-1"
        items={[
          {
            icon: Route,
            label: 'Total encaminhadas',
            value: stats.encaminhados.toLocaleString('pt-BR'),
            secondaryLabel: 'Das triadas',
            secondaryValue: formatPercent(stats.taxaEncaminhamento),
            progressValue: stats.taxaEncaminhamento,
            tone: 'blue',
            tag: 'KPI 07',
            foot: `${stats.encaminhados.toLocaleString('pt-BR')} de ${stats.totalTriagens.toLocaleString('pt-BR')} crianças triadas foram encaminhadas`,
          },
        ]}
      />

      <div className="grid grid-cols-1 gap-5">
        <ChartCard title="Destino do encaminhamento" subtitle="Clique em uma especialidade para priorizar esse recorte">
          <HorizontalBars
            data={specialistData}
            selectedLabel={filters.especialista}
            onSelect={(label) => setFilters((current) => ({ ...current, especialista: current.especialista === label ? undefined : label }))}
          />
        </ChartCard>
      </div>

      <SectionTitle title="Impacto Econômico" tone="amber" />
      <div className="grid grid-cols-1 gap-5 xl:grid-cols-2">
        <ImpactCard
          icon={CheckCircle2}
          title="KPI 10 - Consultas especializadas evitadas"
          value={`${stats.consultasEvitadas.toLocaleString('pt-BR')} consultas evitadas`}
          subtitle="Sem triagem, toda criança iria direto ao especialista."
          detail={`${stats.totalTriagens.toLocaleString('pt-BR')} triadas - ${stats.encaminhados.toLocaleString('pt-BR')} encaminhadas = ${stats.consultasEvitadas.toLocaleString('pt-BR')} evitadas`}
        />
        <ImpactCard
          icon={Banknote}
          title="KPI 11 - Custo estimado com encaminhamentos"
          value={formatCurrency(stats.custoTotalEncaminhamentos)}
          subtitle="Soma dos valores dos especialistas indicados no período."
          detail="Considera apenas avaliações salvas com paciente encaminhado."
          tone="green"
        />
      </div>

      <div className="rounded-xl border border-gray-200 bg-white p-4">
        <h3 className="mb-3 text-sm font-semibold text-gray-700">Triagens recentes</h3>
        <div className="grid grid-cols-1 gap-2 lg:grid-cols-2">
          {stats.ultimasTriagens.map((triagem) => (
            <div key={triagem.idPaciente} className="flex items-center justify-between gap-3 rounded-lg bg-gray-50 p-3">
              <div className="min-w-0">
                <p className="truncate text-sm font-medium">{triagem.nomeCrianca}</p>
                <p className="text-xs text-gray-500">
                  {formatDate(triagem.dataTriagem)} · {triagem.nivelRisco || 'Sem classificação'}
                </p>
              </div>
              <div className="shrink-0 text-right">
                <p className="text-sm font-bold">{triagem.scoreTriagem ?? '-'}/60</p>
                <p className="text-xs text-gray-500">{triagem.encaminhado ? 'Encaminhada' : 'Sem encaminhamento'}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-blue-100 bg-white/80 px-3 py-2">
      <p className="text-xs font-semibold text-gray-500">{label}</p>
      <p className="whitespace-nowrap text-sm font-extrabold text-gray-900">{value}</p>
    </div>
  );
}

function GroupFilter({ value, groups, onChange }: { value?: string; groups: Group[]; onChange: (groupId: string) => void }) {
  return (
    <label className="rounded-xl border border-blue-100 bg-white/80 px-3 py-2">
      <span className="block text-xs font-semibold text-gray-500">Grupo</span>
      <select
        value={value ?? ''}
        onChange={(event) => onChange(event.target.value)}
        className="mt-0.5 w-full min-w-0 bg-transparent text-sm font-extrabold text-gray-900 outline-none"
      >
        <option value="">Todos os grupos</option>
        {groups.map((group) => (
          <option key={group.id} value={group.id}>
            {group.nome}
          </option>
        ))}
      </select>
    </label>
  );
}

function SectionTitle({ title, tone = 'blue' }: { title: string; tone?: 'blue' | 'purple' | 'amber' }) {
  const tones = {
    blue: 'bg-blue-100 text-blue-800',
    purple: 'bg-violet-100 text-violet-800',
    amber: 'bg-amber-100 text-amber-800',
  };

  return (
    <div className="flex items-center gap-3">
      <span className={`rounded-full px-4 py-1.5 text-xs font-extrabold uppercase tracking-wide ${tones[tone]}`}>{title}</span>
      <div className="h-px flex-1 bg-gray-200" />
    </div>
  );
}

function ChartCard({ title, subtitle, children }: { title: string; subtitle: string; children: React.ReactNode }) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
      <h3 className="text-sm font-extrabold text-gray-900">{title}</h3>
      <p className="mb-4 text-xs font-medium text-gray-500">{subtitle}</p>
      {children}
    </div>
  );
}

function ImpactCard({
  icon: Icon,
  title,
  value,
  subtitle,
  detail,
  tone = 'blue',
}: {
  icon: React.ComponentType<{ className?: string }>;
  title: string;
  value: string;
  subtitle: string;
  detail: string;
  tone?: 'blue' | 'green';
}) {
  const style = tone === 'green' ? 'from-emerald-50 to-green-100 border-emerald-100 text-emerald-800' : 'from-blue-50 to-cyan-100 border-blue-100 text-blue-800';

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
      <h3 className="text-sm font-extrabold text-gray-900">{title}</h3>
      <p className="mb-4 text-xs font-medium text-gray-500">{subtitle}</p>
      <div className={`flex items-center gap-4 rounded-xl border bg-gradient-to-br p-4 ${style}`}>
        <Icon className="h-9 w-9 shrink-0" />
        <div>
          <p className="text-2xl font-extrabold tracking-tight">{value}</p>
          <p className="text-xs font-semibold opacity-80">{detail}</p>
        </div>
      </div>
    </div>
  );
}
