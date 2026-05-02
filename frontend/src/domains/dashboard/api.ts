import { isMockMode } from '@/shared/api/client';
import { mockEvaluations as evals } from '@/shared/api/mockData';
import type { Evaluation } from '@/types';

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

export type DashboardFilter = {
  risco?: string;
  especialista?: string;
  dataInicio?: string;
  dataFim?: string;
};

export async function getDashboardStats(filter?: DashboardFilter): Promise<SpiMockSusDashboard> {
  if (!isMockMode()) {
    return (await import('@/shared/api/client')).api.get('/api/dashboard/spi-mock', { params: filter }).then((r) => r.data);
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
  const encaminhados = filteredEvals.filter((e) => e.scoreTotal > 29.5).length;
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
    consultasEvitadas: filteredEvals.length - encaminhados,
    economiaFinanceiraEstimada: (filteredEvals.length - encaminhados) * 1000,
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
      encaminhado: e.scoreTotal > 29.5,
      statusConsulta: 'Sem consulta',
      diagnosticoConfirmado: 'Pendente',
    })),
  };
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

export async function createEvaluation(data: { patientId: number; respostas: Record<number, number>; formId?: number }) {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 600));
    const total = Object.values(data.respostas).reduce((s, v) => s + v, 0);
    const cls = total <= 29.5 ? 'Sem indicativo de TEA' : total < 37 ? 'TEA Leve a Moderado' : 'TEA Grave';
    return {
      id: Date.now(),
      patientId: data.patientId,
      patientNome: 'Novo Paciente',
      avaliadorId: 2,
      avaliadorNome: 'Avaliador',
      respostas: data.respostas,
      scoreTotal: total,
      classificacao: cls,
      dataAvaliacao: new Date().toISOString(),
    } satisfies Evaluation;
  }
  return (await import('@/shared/api/client')).api.post('/api/evaluations', data).then((r) => r.data);
}
