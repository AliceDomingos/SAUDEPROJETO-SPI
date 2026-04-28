import { isMockMode } from '@/shared/api/client';
import { mockEvaluations as evals } from '@/shared/api/mockData';
import type { Evaluation } from '@/types';

export async function getDashboardStats() {
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
