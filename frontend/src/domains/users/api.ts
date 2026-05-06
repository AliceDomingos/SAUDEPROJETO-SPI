import { isMockMode, api } from '@/shared/api/client';
import { mockUsers } from '@/shared/api/mockData';
import type { UserRole, User } from '@/types';

export interface CreateUserInput {
  nome: string;
  email: string;
  role: UserRole;
  groupIds?: string[];
}

export interface UpdateUserInput {
  nome: string;
  email: string;
  role: UserRole;
  groupIds?: string[];
}

export async function getUsers() {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 300));
    return mockUsers;
  }
  return (await api.get('/api/users')).data;
}

export async function createUser(data: CreateUserInput) {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 300));

    const u = {
      id: crypto.randomUUID(),
      nome: data.nome,
      email: data.email,
      role: data.role,
      ativo: true,
      groupIds: data.groupIds ?? [],
      groupNames: [],
      criado_em: new Date().toISOString(),
    } satisfies User;
    mockUsers.push(u);
    return u;
  }
  return (await api.post('/api/auth/register', data)).data;
}

export async function updateUser(id: string, data: UpdateUserInput) {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 300));
    const user = mockUsers.find((u) => u.id === id);
    if (!user) {
      throw new Error('Usuario nao encontrado');
    }

    user.nome = data.nome;
    user.email = data.email;
    user.role = data.role;
    if (data.groupIds !== undefined) {
      user.groupIds = data.groupIds;
    }
    return user;
  }

  if (data.groupIds !== undefined) {
    await api.put(`/api/users/${id}/groups`, { groupIds: data.groupIds });
  }
}

export async function deactivateUser(id: string) {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 300));
    const user = mockUsers.find((u) => u.id === id);
    if (user) user.ativo = false;
    return;
  }
  await api.put(`/api/users/${id}/deactivate`);
}

export async function sendUserPasswordInvite(id: string) {
  if (isMockMode()) {
    await new Promise((r) => setTimeout(r, 300));
    const user = mockUsers.find((u) => u.id === id);
    if (!user) {
      throw new Error('Usuario nao encontrado');
    }
    return;
  }

  await api.post(`/api/auth/users/${id}/password-invite`);
}
