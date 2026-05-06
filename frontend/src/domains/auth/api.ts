import axios from 'axios';
import { isMockMode, api, getApiUrlCandidates, setApiBaseUrl } from '@/shared/api/client';
import type { LoginRequest, SetPasswordFromInviteRequest, TokenResponse } from './types';

const INVALID_CREDENTIALS_MESSAGE =
  'Infelizmente, nao foi possivel fazer login com esse usuario. As credenciais devem estar incorretas.';

function normalizeLoginResponse(payload: unknown): TokenResponse {
  const res = payload as Record<string, unknown>;
  const rawUser = (res.user ?? res.User ?? {}) as Record<string, unknown>;

  return {
    access_token: String(res.access_token ?? res.AccessToken ?? ''),
    user: {
      id: String(rawUser.id ?? rawUser.Id ?? ''),
      nome: String(rawUser.nome ?? rawUser.Nome ?? ''),
      email: String(rawUser.email ?? rawUser.Email ?? ''),
      role: String(rawUser.role ?? rawUser.Role ?? '') as TokenResponse['user']['role'],
      ativo: Boolean(rawUser.ativo ?? rawUser.Ativo ?? false),
      groupIds: normalizeStringArray(rawUser.groupIds ?? rawUser.GroupIds ?? rawUser.group_ids),
      groupNames: normalizeStringArray(rawUser.groupNames ?? rawUser.GroupNames ?? rawUser.group_names),
      criado_em: String(rawUser.criado_em ?? rawUser.criadoEm ?? rawUser.CriadoEm ?? ''),
    },
  };
}

function toAuthError(error: unknown) {
  if (!axios.isAxiosError<{ detail?: string }>(error)) {
    return error instanceof Error ? error : new Error('Erro ao fazer login');
  }

  const detail = error.response?.data?.detail?.trim();
  if (detail) {
    if (detail.toLowerCase().includes('credenciais')) {
      return new Error(INVALID_CREDENTIALS_MESSAGE);
    }

    return new Error(detail);
  }

  if (error.code === 'ERR_NETWORK') {
    return new Error('Nao foi possivel conectar na API. Verifique se o backend esta rodando.');
  }

  return new Error(error.message || 'Erro ao fazer login');
}

export async function loginReq(data: LoginRequest): Promise<TokenResponse> {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 600));
    if (data.password === 'admin123') {
      return {
        access_token: 'mock-jwt-token',
        user: {
          id: crypto.randomUUID(),
          nome: 'Administrador',
          email: data.email,
          role: 'admin',
          ativo: true,
          groupIds: [],
          groupNames: [],
          criado_em: new Date().toISOString(),
        },
      };
    }
    throw new Error(INVALID_CREDENTIALS_MESSAGE);
  }

  try {
    const { data: res } = await api.post('/api/auth/login', data);
    return normalizeLoginResponse(res);
  } catch (error) {
    if (!axios.isAxiosError(error) || error.response || error.code !== 'ERR_NETWORK') {
      throw toAuthError(error);
    }

    const currentBaseUrl = String(api.defaults.baseURL ?? '').trim().replace(/\/+$/, '');
    for (const candidateUrl of getApiUrlCandidates()) {
      if (candidateUrl === currentBaseUrl) {
        continue;
      }

      try {
        const { data: res } = await axios.post(`${candidateUrl}/api/auth/login`, data, {
          headers: { 'Content-Type': 'application/json' },
        });

        setApiBaseUrl(candidateUrl);
        return normalizeLoginResponse(res);
      } catch (candidateError) {
        if (axios.isAxiosError(candidateError) && candidateError.response) {
          setApiBaseUrl(candidateUrl);
          throw toAuthError(candidateError);
        }
      }
    }

    throw toAuthError(error);
  }
}

export async function setPasswordFromInvite(data: SetPasswordFromInviteRequest) {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 600));

    if (!data.token.trim()) {
      throw new Error('Convite invalido.');
    }

    if (data.password.trim().length < 6) {
      throw new Error('A senha precisa ter pelo menos 6 caracteres.');
    }

    return;
  }

  await api.post('/api/auth/set-password', data);
}

function normalizeStringArray(value: unknown): string[] {
  if (!Array.isArray(value)) {
    return [];
  }

  return value
    .map((item) => String(item).trim())
    .filter(Boolean);
}
