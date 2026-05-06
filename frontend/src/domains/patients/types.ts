import type { Patient } from '@/types';

export type { Patient };

export type PatientSex = 'masculino' | 'feminino' | 'outro';
export type PatientFormSex = '' | PatientSex;
export type PatientSearchField = 'all' | 'nome' | 'cpf' | 'sexo' | 'data_nascimento';

export interface PatientFormValues {
  nome: string;
  cpf: string;
  dataNascimento: string;
  sexo: PatientFormSex;
  groupId: string;
  nomeResponsavel: string;
  telefone: string;
  email: string;
  cep: string;
  estado: string;
  cidade: string;
  bairro: string;
  rua: string;
  numero: string;
  complemento: string;
  observacoes: string;
}

export interface PatientUpsertInput {
  nome: string;
  cpf: string;
  data_nascimento: string;
  sexo: PatientSex;
  groupId?: string;
  nome_responsavel?: string;
  telefone?: string;
  email?: string;
  cep?: string;
  estado?: string;
  cidade?: string;
  bairro?: string;
  rua?: string;
  numero?: string;
  complemento?: string;
  observacoes?: string;
}
