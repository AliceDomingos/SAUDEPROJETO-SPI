// Shared domain types
export type UserRole = 'admin' | 'analista' | 'agente_saude' | 'gestor';

export interface User {
  id: string;
  nome: string;
  email: string;
  role: UserRole;
  ativo: boolean;
  podeAvaliar?: boolean;
  groupIds?: string[];
  groupNames?: string[];
  criado_em: string;
}

export interface Patient {
  id: string;
  nome: string;
  idade: number | null;
  avaliador_id: string | null;
  cpf?: string | null;
  data_nascimento?: string | null;
  sexo?: 'masculino' | 'feminino' | 'outro' | null;
  nome_responsavel?: string | null;
  telefone?: string | null;
  email?: string | null;
  cep?: string | null;
  estado?: string | null;
  cidade?: string | null;
  bairro?: string | null;
  rua?: string | null;
  numero?: string | null;
  complemento?: string | null;
  observacoes?: string | null;
  group_id?: string | null;
  group_nome?: string | null;
  criado_em: string;
}

export interface Evaluation {
  id: string;
  patientId: string;
  patientNome: string;
  avaliadorId: string;
  avaliadorNome: string;
  formId?: number;
  formNome?: string;
  respostas: Record<string, number>;
  scoreTotal: number;
  pesoTotal: number;
  classificacao: string;
  observacoes?: string | null;
  dataAvaliacao: string;
  referral?: EvaluationReferral | null;
}

export interface EvaluationReferral {
  id: string;
  evaluationId: string;
  patientId: string;
  encaminhado: boolean;
  specialistId: string | null;
  specialistNome: string | null;
  especialidade: string | null;
  custoEstimado: number;
  criadoEm: string;
}

export interface DashboardStats {
  total: number;
  averageScore: number;
  lastMonthEvaluations: number;
  classificationDistribution: {
    semIndicativo: number;
    teaLeveModerado: number;
    teaGrave: number;
  };
  recentEvaluations: Evaluation[];
}
