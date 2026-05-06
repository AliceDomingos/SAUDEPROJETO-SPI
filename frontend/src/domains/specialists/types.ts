export interface Specialist {
  id: string;
  nome: string;
  especialidade: string;
  custoConsulta: number;
  ativo: boolean;
  criadoEm: string;
}

export interface SpecialistFormValues {
  nome: string;
  especialidade: string;
  custoConsulta: string;
  ativo: boolean;
}

export interface SpecialistUpsertInput {
  nome: string;
  especialidade: string;
  custoConsulta: number;
  ativo?: boolean;
}
