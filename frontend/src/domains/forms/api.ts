import { api } from '@/shared/api/client';
import type { Formulario, CriarFormularioPayload, Grupo } from './types';

export async function getForms(): Promise<Formulario[]> {
  return api.get('/api/forms').then((r) => r.data);
}

export async function getFormById(id: number): Promise<Formulario> {
  return api.get(`/api/forms/${id}`).then((r) => r.data);
}

export async function createForm(data: CriarFormularioPayload): Promise<Formulario> {
  return api.post('/api/forms', data).then((r) => r.data);
}

export async function updateForm(id: number, data: CriarFormularioPayload): Promise<Formulario> {
  return api.put(`/api/forms/${id}`, data).then((r) => r.data);
}

export async function deactivateForm(id: number): Promise<void> {
  return api.delete(`/api/forms/${id}`).then(() => {});
}

export async function activateForm(id: number): Promise<void> {
  return api.patch(`/api/forms/${id}/activate`).then(() => {});
}

export async function getGrupos(): Promise<Grupo[]> {
  return api.get('/api/groups').then((r) => r.data);
}
