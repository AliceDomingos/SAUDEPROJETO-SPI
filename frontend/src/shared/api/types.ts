export interface ApiResponse<T> {
  data: T;
  message?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  perPage: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface TokenResponse {
  access_token: string;
  user: {
    id: string;
    nome: string;
    email: string;
    role: 'admin' | 'analista' | 'agente_saude' | 'gestor' | 'avaliador';
    ativo: boolean;
    groupIds: string[];
    groupNames: string[];
    criado_em: string;
  };
}

export interface PatientCreate {
  nome: string;
  idade?: number;
}

export interface EvaluationCreate {
  patient_id: string;
  respostas: Record<string, number>;
}
