import type { UserRole } from '@/types';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SetPasswordFromInviteRequest {
  token: string;
  password: string;
}

export interface TokenResponse {
  access_token: string;
  user: {
    id: string;
    nome: string;
    email: string;
    role: UserRole;
    ativo: boolean;
    groupIds: string[];
    groupNames: string[];
    criado_em: string;
  };
}
