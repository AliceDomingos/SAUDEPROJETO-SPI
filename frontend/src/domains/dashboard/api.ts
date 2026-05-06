import { isMockMode } from '@/shared/api/client';
import { mockEvaluations as evals, mockGroups, mockSpecialists } from '@/shared/api/mockData';
import type { Group } from '@/domains/groups/types';
import type { Evaluation, EvaluationReferral } from '@/types';

export interface DashboardDistributionItem {
  label: string;
  value: number;
}

export interface SpiMockSusTriage {
  idPaciente: string;
  nomeCrianca: string;
  idadeAnos: number | null;
  sexo: string;
  dataTriagem: string | null;
  scoreTriagem: number | null;
  nivelRisco: string;
  encaminhado: boolean;
  statusConsulta: string;
  diagnosticoConfirmado: string;
}

export interface SpiMockSusDashboard {
  fonte: string;
  aba: string;
  atualizadoEm: string;
  totalPacientes: number;
  totalTriagens: number;
  triagensMesAtual: number;
  scoreMedio: number;
  menorScore: number;
  maiorScore: number;
  encaminhados: number;
  consultasAgendadas: number;
  consultasRealizadas: number;
  consultasCanceladas: number;
  diagnosticosConfirmados: number;
  tratamentosIniciados: number;
  casosSeveros: number;
  tempoMedioEsperaDias: number;
  tempoMedioIntervencaoDias: number;
  consultasEvitadas: number;
  economiaFinanceiraEstimada: number;
  custoTotalEncaminhamentos: number;
  custoMedioConsultaEspecializada: number;
  taxaEncaminhamento: number;
  taxaComparecimento: number;
  taxaDiagnosticoConfirmado: number;
  taxaEncaminhamentoAssertivo: number;
  taxaTratamentoAposDiagnostico: number;
  periodoTriagens: {
    inicio: string | null;
    fim: string | null;
  };
  distribuicaoTriagensMensais: DashboardDistributionItem[];
  distribuicaoRisco: DashboardDistributionItem[];
  distribuicaoStatusConsulta: DashboardDistributionItem[];
  distribuicaoEspecialista: DashboardDistributionItem[];
  distribuicaoDiagnostico: DashboardDistributionItem[];
  distribuicaoTratamento: DashboardDistributionItem[];
  ultimasTriagens: SpiMockSusTriage[];
}

interface SystemDashboardSummary {
  triagens?: {
    totalPacientes: number;
    totalTriagens: number;
    scoreMedio: number;
    menorScore: number;
    maiorScore: number;
    encaminhados: number;
    consultasEvitadas: number;
    economiaFinanceiraEstimada: number;
    custoTotalEncaminhamentos: number;
    casosSeveros: number;
    taxaEncaminhamento: number;
    distribuicaoTriagensMensais: DashboardDistributionItem[];
    distribuicaoRisco: DashboardDistributionItem[];
    distribuicaoEspecialista: DashboardDistributionItem[];
  };
  ultimasAvaliacoes?: Evaluation[];
}

export type DashboardFilter = {
  risco?: string;
  especialista?: string;
  dataInicio?: string;
  dataFim?: string;
  grupoId?: string;
};

export async function getDashboardStats(filter?: DashboardFilter): Promise<SpiMockSusDashboard> {
  if (!isMockMode()) {
    const { api } = await import('@/shared/api/client');
    const { data } = await api.get('/api/dashboard', { params: filter });
    return dashboardFromSystemSummary(data);
  }

  const filteredEvals = evals.filter((e) => {
    if (filter?.risco) {
      const risco = filter.risco?.toLocaleLowerCase('pt-BR') ?? '';
      if (risco.includes('severo') && e.scoreTotal < 37) return false;
      if ((risco.includes('leve') || risco.includes('moderado')) && (e.scoreTotal <= 29.5 || e.scoreTotal >= 37)) return false;
      if (risco.includes('sem') && e.scoreTotal > 29.5) return false;
    }

    const evaluationDate = new Date(e.dataAvaliacao);
    if (filter?.dataInicio && evaluationDate < new Date(`${filter.dataInicio}T00:00:00`)) {
      return false;
    }

    if (filter?.dataFim && evaluationDate > new Date(`${filter.dataFim}T23:59:59`)) {
      return false;
    }

    return true;
  });
  const avg = filteredEvals.reduce((s, e) => s + e.scoreTotal, 0) / (filteredEvals.length || 1);
  const severe = filteredEvals.filter((e) => e.scoreTotal >= 37).length;
  const encaminhados = filteredEvals.filter((e) => e.referral?.encaminhado === true).length;
  const custoTotalEncaminhamentos = filteredEvals
    .filter((e) => e.referral?.encaminhado === true)
    .reduce((sum, evaluation) => sum + Number(evaluation.referral?.custoEstimado ?? 0), 0);
  const scores = filteredEvals.map((e) => e.scoreTotal);

  return {
    fonte: 'mockData.ts',
    aba: 'avaliacoes',
    atualizadoEm: new Date().toISOString(),
    totalPacientes: new Set(filteredEvals.map((e) => e.patientId)).size,
    totalTriagens: filteredEvals.length,
    triagensMesAtual: 2,
    scoreMedio: Math.round(avg * 10) / 10,
    menorScore: scores.length ? Math.min(...scores) : 0,
    maiorScore: scores.length ? Math.max(...scores) : 0,
    encaminhados,
    consultasAgendadas: 0,
    consultasRealizadas: 0,
    consultasCanceladas: 0,
    diagnosticosConfirmados: severe,
    tratamentosIniciados: 0,
    casosSeveros: severe,
    tempoMedioEsperaDias: 0,
    tempoMedioIntervencaoDias: 0,
    consultasEvitadas: filteredEvals.filter((e) => e.referral && !e.referral.encaminhado).length,
    economiaFinanceiraEstimada: 0,
    custoTotalEncaminhamentos,
    custoMedioConsultaEspecializada: 1000,
    taxaEncaminhamento: filteredEvals.length ? Math.round((encaminhados * 1000) / filteredEvals.length) / 10 : 0,
    taxaComparecimento: 0,
    taxaDiagnosticoConfirmado: 0,
    taxaEncaminhamentoAssertivo: 0,
    taxaTratamentoAposDiagnostico: 0,
    periodoTriagens: { inicio: null, fim: null },
    distribuicaoTriagensMensais: [],
    distribuicaoRisco: [
      { label: 'Sem sinais', value: filteredEvals.filter((e) => e.scoreTotal <= 29.5).length },
      { label: 'Leve/Moderado', value: filteredEvals.filter((e) => e.scoreTotal > 29.5 && e.scoreTotal < 37).length },
      { label: 'Severo', value: severe },
    ],
    distribuicaoStatusConsulta: [],
    distribuicaoEspecialista: [],
    distribuicaoDiagnostico: [],
    distribuicaoTratamento: [],
    ultimasTriagens: filteredEvals.slice(-6).reverse().map((e) => ({
      idPaciente: String(e.patientId),
      nomeCrianca: e.patientNome,
      idadeAnos: null,
      sexo: '',
      dataTriagem: e.dataAvaliacao,
      scoreTriagem: e.scoreTotal,
      nivelRisco: e.classificacao,
      encaminhado: Boolean(e.referral?.encaminhado),
      statusConsulta: 'Sem consulta',
      diagnosticoConfirmado: 'Pendente',
    })),
  };
}

function dashboardFromSystemSummary(system: SystemDashboardSummary): SpiMockSusDashboard {
  const summary = system.triagens;
  const latestEvaluations = system.ultimasAvaliacoes ?? [];
  const latestMonth = summary?.distribuicaoTriagensMensais?.at(-1);

  return {
    fonte: 'banco de dados',
    aba: 'Sistema',
    atualizadoEm: new Date().toISOString(),
    totalPacientes: summary?.totalPacientes ?? 0,
    totalTriagens: summary?.totalTriagens ?? 0,
    triagensMesAtual: latestMonth?.value ?? 0,
    scoreMedio: Number(summary?.scoreMedio ?? 0),
    menorScore: Number(summary?.menorScore ?? 0),
    maiorScore: Number(summary?.maiorScore ?? 0),
    encaminhados: summary?.encaminhados ?? 0,
    consultasAgendadas: 0,
    consultasRealizadas: 0,
    consultasCanceladas: 0,
    diagnosticosConfirmados: 0,
    tratamentosIniciados: 0,
    casosSeveros: summary?.casosSeveros ?? 0,
    tempoMedioEsperaDias: 0,
    tempoMedioIntervencaoDias: 0,
    consultasEvitadas: summary?.consultasEvitadas ?? 0,
    economiaFinanceiraEstimada: Number(summary?.economiaFinanceiraEstimada ?? 0),
    custoTotalEncaminhamentos: Number(summary?.custoTotalEncaminhamentos ?? 0),
    custoMedioConsultaEspecializada: 0,
    taxaEncaminhamento: Number(summary?.taxaEncaminhamento ?? 0),
    taxaComparecimento: 0,
    taxaDiagnosticoConfirmado: 0,
    taxaEncaminhamentoAssertivo: 0,
    taxaTratamentoAposDiagnostico: 0,
    periodoTriagens: { inicio: null, fim: null },
    distribuicaoTriagensMensais: summary?.distribuicaoTriagensMensais ?? [],
    distribuicaoRisco: summary?.distribuicaoRisco ?? [],
    distribuicaoStatusConsulta: [],
    distribuicaoEspecialista: summary?.distribuicaoEspecialista ?? [],
    distribuicaoDiagnostico: [],
    distribuicaoTratamento: [],
    ultimasTriagens: latestEvaluations.map((evaluation) => ({
      idPaciente: evaluation.patientId,
      nomeCrianca: evaluation.patientNome,
      idadeAnos: null,
      sexo: '',
      dataTriagem: evaluation.dataAvaliacao,
      scoreTriagem: Number(evaluation.scoreTotal),
      nivelRisco: classifyRiskLabel(evaluation),
      encaminhado: Boolean(evaluation.referral?.encaminhado),
      statusConsulta: evaluation.referral?.encaminhado ? 'Encaminhada' : evaluation.referral ? 'Sem encaminhamento' : 'Sem decisao',
      diagnosticoConfirmado: 'Pendente',
    })),
  };
}

export async function getDashboardGroups(): Promise<Group[]> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 200));
    return mockGroups.map(normalizeDashboardGroup);
  }

  const { api } = await import('@/shared/api/client');
  const { data } = await api.get('/api/dashboard/groups');
  return Array.isArray(data) ? data.map(normalizeDashboardGroup) : [];
}

export async function saveEvaluationReferral(
  evaluationId: string,
  data: { encaminhado: boolean; specialistId?: string | null }
): Promise<EvaluationReferral> {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 350));
    const specialist = data.specialistId ? mockSpecialists.find((item) => item.id === data.specialistId) ?? null : null;
    const referral = {
      id: crypto.randomUUID(),
      evaluationId,
      patientId: '',
      encaminhado: data.encaminhado,
      specialistId: data.encaminhado ? specialist?.id ?? data.specialistId ?? null : null,
      specialistNome: data.encaminhado ? specialist?.nome ?? null : null,
      especialidade: data.encaminhado ? specialist?.especialidade ?? null : null,
      custoEstimado: data.encaminhado ? specialist?.custoConsulta ?? 0 : 0,
      criadoEm: new Date().toISOString(),
    };

    const evaluation = evals.find((item) => item.id === evaluationId);
    if (evaluation) {
      evaluation.referral = {
        ...referral,
        patientId: evaluation.patientId,
      };
    }

    return evaluation?.referral ?? referral;
  }

  return (await import('@/shared/api/client')).api.put(`/api/evaluations/${evaluationId}/referral`, data).then((r) => r.data);
}

export async function getEvaluationDashboardStats() {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 300));
    const avg = evals.reduce((s, e) => s + e.scoreTotal, 0) / (evals.length || 1);
    return {
      total: evals.length,
      averageScore: Math.round(avg * 10) / 10,
      lastMonth: 2,
      classificationDistribution: {
        semIndicativo: evals.filter((e) => e.scoreTotal <= 29.5).length,
        teaLeveModerado: evals.filter((e) => e.scoreTotal > 29.5 && e.scoreTotal < 37).length,
        teaGrave: evals.filter((e) => e.scoreTotal >= 37).length,
      },
      recentEvaluations: evals.slice(-4).reverse(),
    };
  }
  return (await import('@/shared/api/client')).api.get('/api/evaluations/stats').then((r) => r.data);
}

export async function getEvals(): Promise<Evaluation[]> {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 300));
    return evals;
  }
  return (await import('@/shared/api/client')).api.get('/api/evaluations').then((r) => r.data);
}

export async function createEvaluation(data: { patientId: string; respostas: Record<string, number>; formId?: string; groupId?: string; observacoes?: string }) {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 600));
    const total = Object.values(data.respostas).reduce((s, v) => s + v, 0);
    const cls = total <= 29.5 ? 'Sem indicativo de TEA' : total < 37 ? 'TEA Leve a Moderado' : 'TEA Grave';
    return {
      id: crypto.randomUUID(),
      patientId: data.patientId,
      patientNome: 'Novo Paciente',
      avaliadorId: '',
      avaliadorNome: 'Avaliador',
      respostas: data.respostas,
      scoreTotal: total,
      classificacao: cls,
      observacoes: data.observacoes?.trim() || null,
      dataAvaliacao: new Date().toISOString(),
    } satisfies Evaluation;
  }
  return (await import('@/shared/api/client')).api.post('/api/evaluations', data).then((r) => r.data);
}

function normalizeDashboardGroup(payload: unknown): Group {
  const raw = payload as Record<string, unknown>;

  return {
    id: nullableString(raw.id ?? raw.Id) ?? '',
    nome: String(raw.nome ?? raw.Nome ?? ''),
    gestor_id: nullableString(raw.gestor_id ?? raw.gestorId ?? raw.GestorId),
    gestor_nome: nullableString(raw.gestor_nome ?? raw.gestorNome ?? raw.GestorNome),
    ativo: Boolean(raw.ativo ?? raw.Ativo ?? false),
    quantidade_membros: Number(raw.quantidade_membros ?? raw.quantidadeMembros ?? raw.QuantidadeMembros ?? 0),
    criado_em: String(raw.criado_em ?? raw.criadoEm ?? raw.CriadoEm ?? new Date().toISOString()),
  };
}

function nullableString(value: unknown): string | null {
  if (value == null) return null;
  const normalized = String(value).trim();
  return normalized ? normalized : null;
}

function nullableNumber(value: unknown): number | null {
  if (value == null || value === '') return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function classifyRiskLabel(evaluation: Evaluation) {
  const classification = evaluation.classificacao.toLocaleLowerCase('pt-BR');
  if (classification.includes('grave')) return 'Severo';
  if (classification.includes('moderado')) return 'Moderado';
  if (classification.includes('leve')) return 'Leve';
  if (classification.includes('sem')) return 'Sem Sinais';
  const score = Number(evaluation.scoreTotal);
  if (score >= 37) return 'Severo';
  if (score > 29.5) return 'Moderado';
  return 'Sem Sinais';
}
