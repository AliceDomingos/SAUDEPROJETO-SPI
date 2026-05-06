import { isMockMode, api } from '@/shared/api/client';
import { mockPatients } from '@/shared/api/mockData';
import type { Evaluation, Patient } from '@/types';
import { getEvals } from '@/domains/dashboard/api';
import type { PatientUpsertInput } from './types';

export type CreatePatientInput = PatientUpsertInput;
export type UpdatePatientInput = PatientUpsertInput;

export interface CepLookupResult {
  cep: string;
  estado: string;
  cidade: string;
  bairro: string;
  rua: string;
  complemento: string;
}

export async function getPatients(): Promise<Patient[]> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 300));
    return mockPatients.map(normalizePatient);
  }

  const { data } = await api.get('/api/patients');
  return Array.isArray(data) ? data.map(normalizePatient) : [];
}

export async function getReusablePatients(): Promise<Patient[]> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 300));
    return mockPatients.map(normalizePatient);
  }

  const { data } = await api.get('/api/patients/reusable');
  return Array.isArray(data) ? data.map(normalizePatient) : [];
}

export async function createPatient(data: CreatePatientInput): Promise<Patient> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const created = normalizePatient({
      id: crypto.randomUUID(),
      nome: data.nome,
      cpf: data.cpf,
      data_nascimento: data.data_nascimento,
      sexo: data.sexo,
      nome_responsavel: data.nome_responsavel || null,
      telefone: data.telefone || null,
      email: data.email || null,
      cep: data.cep || null,
      estado: data.estado || null,
      cidade: data.cidade || null,
      bairro: data.bairro || null,
      rua: data.rua || null,
      numero: data.numero || null,
      complemento: data.complemento || null,
      observacoes: data.observacoes || null,
      idade: calculateAge(data.data_nascimento),
      avaliador_id: null,
      group_id: data.groupId ?? null,
      group_nome: 'Grupo Padrao',
      criado_em: new Date().toISOString(),
    });

    mockPatients.push(created);
    return created;
  }

  const response = await api.post('/api/patients', data);
  return normalizePatient(response.data);
}

export async function updatePatient(id: string, data: UpdatePatientInput): Promise<Patient> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const patient = mockPatients.find((item) => item.id === id);
    if (!patient) {
      throw new Error('Paciente nao encontrado.');
    }

    patient.nome = data.nome;
    patient.cpf = data.cpf;
    patient.data_nascimento = data.data_nascimento;
    patient.sexo = data.sexo;
    patient.nome_responsavel = data.nome_responsavel || null;
    patient.telefone = data.telefone || null;
    patient.email = data.email || null;
    patient.cep = data.cep || null;
    patient.estado = data.estado || null;
    patient.cidade = data.cidade || null;
    patient.bairro = data.bairro || null;
    patient.rua = data.rua || null;
    patient.numero = data.numero || null;
    patient.complemento = data.complemento || null;
    patient.observacoes = data.observacoes || null;
    patient.idade = calculateAge(data.data_nascimento);
    return normalizePatient(patient);
  }

  const response = await api.put(`/api/patients/${id}`, data);
  return normalizePatient(response.data);
}

export async function deletePatient(id: string): Promise<void> {
  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 300));

    const index = mockPatients.findIndex((item) => item.id === id);
    if (index >= 0) {
      mockPatients.splice(index, 1);
    }
    return;
  }

  await api.delete(`/api/patients/${id}`);
}

export async function getPatientEvaluations(patientId: string): Promise<Evaluation[]> {
  const evaluations = await getEvals();
  return evaluations.filter((item) => item.patientId === patientId);
}

export async function lookupAddressByCep(rawCep: string): Promise<CepLookupResult> {
  const cep = rawCep.replace(/\D/g, '');

  if (cep.length !== 8) {
    throw new Error('Informe um CEP com 8 digitos para buscar o endereco.');
  }

  if (isMockMode()) {
    await new Promise((resolve) => setTimeout(resolve, 300));

    return {
      cep,
      estado: 'SP',
      cidade: 'Sao Paulo',
      bairro: 'Centro',
      rua: 'Rua Exemplo',
      complemento: '',
    };
  }

  const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
  if (!response.ok) {
    throw new Error('Nao foi possivel consultar o CEP agora.');
  }

  const payload = (await response.json()) as Record<string, unknown>;
  if (payload.erro === true) {
    throw new Error('CEP nao encontrado.');
  }

  return {
    cep,
    estado: asNullableString(payload.uf) ?? '',
    cidade: asNullableString(payload.localidade) ?? '',
    bairro: asNullableString(payload.bairro) ?? '',
    rua: asNullableString(payload.logradouro) ?? '',
    complemento: asNullableString(payload.complemento) ?? '',
  };
}

function normalizePatient(payload: unknown): Patient {
  const raw = payload as Record<string, unknown>;
  const birthDate = raw.data_nascimento ?? raw.dataNascimento ?? null;
  const rawSex = asNullableString(raw.sexo ?? raw.Sexo);
  const sexo = rawSex === 'feminino' || rawSex === 'masculino' || rawSex === 'outro' ? rawSex : null;

  return {
    id: asNullableString(raw.id ?? raw.Id) ?? '',
    nome: String(raw.nome ?? raw.Nome ?? ''),
    idade: resolveNullableNumber(raw.idade ?? raw.Idade, calculateAge(asNullableString(birthDate))),
    avaliador_id: asNullableString(raw.avaliador_id ?? raw.avaliadorId ?? raw.AvaliadorId),
    cpf: asNullableString(raw.cpf ?? raw.Cpf),
    data_nascimento: asNullableString(birthDate),
    sexo,
    nome_responsavel: asNullableString(raw.nome_responsavel ?? raw.nomeResponsavel ?? raw.NomeResponsavel),
    telefone: asNullableString(raw.telefone ?? raw.Telefone),
    email: asNullableString(raw.email ?? raw.Email),
    cep: asNullableString(raw.cep ?? raw.Cep),
    estado: asNullableString(raw.estado ?? raw.Estado),
    cidade: asNullableString(raw.cidade ?? raw.Cidade),
    bairro: asNullableString(raw.bairro ?? raw.Bairro),
    rua: asNullableString(raw.rua ?? raw.Rua),
    numero: asNullableString(raw.numero ?? raw.Numero),
    complemento: asNullableString(raw.complemento ?? raw.Complemento),
    observacoes: asNullableString(raw.observacoes ?? raw.Observacoes),
    group_id: asNullableString(raw.group_id ?? raw.groupId ?? raw.GroupId),
    group_nome: asNullableString(raw.group_nome ?? raw.groupNome ?? raw.GroupNome),
    criado_em: String(raw.criado_em ?? raw.criadoEm ?? raw.CriadoEm ?? new Date().toISOString()),
  };
}

function calculateAge(value?: string | null): number | null {
  if (!value) {
    return null;
  }

  const birthDate = new Date(value);
  if (Number.isNaN(birthDate.getTime())) {
    return null;
  }

  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  const monthDiff = today.getMonth() - birthDate.getMonth();
  const dayDiff = today.getDate() - birthDate.getDate();

  if (monthDiff < 0 || (monthDiff === 0 && dayDiff < 0)) {
    age -= 1;
  }

  return age >= 0 ? age : null;
}

function asNullableString(value: unknown): string | null {
  if (value == null) {
    return null;
  }

  const normalized = String(value).trim();
  return normalized ? normalized : null;
}

function resolveNullableNumber(value: unknown, fallback: number | null = null): number | null {
  if (value == null || value === '') {
    return fallback;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : fallback;
}
