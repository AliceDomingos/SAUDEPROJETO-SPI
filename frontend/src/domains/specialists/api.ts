import { api, isMockMode } from '@/shared/api/client';
import { mockSpecialists } from '@/shared/api/mockData';
import type { Specialist, SpecialistUpsertInput } from './types';

export type CreateSpecialistInput = SpecialistUpsertInput;
export type UpdateSpecialistInput = SpecialistUpsertInput;

export async function getSpecialists(options?: { activeOnly?: boolean }): Promise<Specialist[]> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 250));
    const list = options?.activeOnly ? mockSpecialists.filter((item) => item.ativo) : mockSpecialists;
    return list.map(normalizeSpecialist);
  }

  const { data } = await api.get('/api/specialists', { params: { activeOnly: options?.activeOnly ?? false } });
  return Array.isArray(data) ? data.map(normalizeSpecialist) : [];
}

export async function createSpecialist(data: CreateSpecialistInput): Promise<Specialist> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 250));
    const created = normalizeSpecialist({
      id: crypto.randomUUID(),
      nome: data.nome,
      especialidade: data.especialidade,
      custoConsulta: data.custoConsulta,
      ativo: true,
      criadoEm: new Date().toISOString(),
    });
    mockSpecialists.push(created);
    return created;
  }

  const response = await api.post('/api/specialists', data);
  return normalizeSpecialist(response.data);
}

export async function updateSpecialist(id: string, data: UpdateSpecialistInput): Promise<Specialist> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 250));
    const specialist = mockSpecialists.find((item) => item.id === id);
    if (!specialist) {
      throw new Error('Especialista nao encontrado.');
    }

    specialist.nome = data.nome;
    specialist.especialidade = data.especialidade;
    specialist.custoConsulta = data.custoConsulta;
    specialist.ativo = data.ativo ?? true;
    return normalizeSpecialist(specialist);
  }

  const response = await api.put(`/api/specialists/${id}`, data);
  return normalizeSpecialist(response.data);
}

export async function deactivateSpecialist(id: string): Promise<void> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 250));
    const specialist = mockSpecialists.find((item) => item.id === id);
    if (specialist) {
      specialist.ativo = false;
    }
    return;
  }

  await api.delete(`/api/specialists/${id}`);
}

function normalizeSpecialist(payload: unknown): Specialist {
  const raw = payload as Record<string, unknown>;
  return {
    id: String(raw.id ?? raw.Id ?? ''),
    nome: String(raw.nome ?? raw.Nome ?? ''),
    especialidade: String(raw.especialidade ?? raw.Especialidade ?? ''),
    custoConsulta: Number(raw.custoConsulta ?? raw.CustoConsulta ?? raw.custo_consulta ?? 0),
    ativo: Boolean(raw.ativo ?? raw.Ativo ?? false),
    criadoEm: String(raw.criadoEm ?? raw.CriadoEm ?? raw.criado_em ?? new Date().toISOString()),
  };
}
