import type { User } from '@/types';
import type { Group, GroupFormValues, GroupUpsertInput } from '../../types';

export function buildGroupFormValues(group?: Group | null): GroupFormValues {
  return {
    nome: group?.nome ?? '',
    gestorId: group?.gestor_id ? String(group.gestor_id) : '',
  };
}

export function mapGroupFormToInput(values: GroupFormValues): GroupUpsertInput {
  return {
    nome: values.nome.trim(),
    gestorId: values.gestorId ? Number(values.gestorId) : null,
  };
}

export function validateGroupForm(
  values: GroupFormValues,
  managers: User[],
  _requireManager: boolean,
): string | null {
  if (!values.nome.trim()) {
    return 'Informe o nome do grupo.';
  }

  if (values.gestorId) {
    const selectedManagerId = Number(values.gestorId);
    if (!managers.some((manager) => manager.id === selectedManagerId)) {
      return 'Selecione um gestor valido.';
    }
  }

  return null;
}

export function formatGroupDate(value: string): string {
  return new Date(value).toLocaleDateString('pt-BR');
}

export function getGroupSearchText(group: Group): string {
  return [group.nome, group.gestor_nome]
    .filter(Boolean)
    .join(' ')
    .toLowerCase();
}

export function getGroupStatusLabel(active: boolean): string {
  return active ? 'Ativo' : 'Inativo';
}

export function getGroupStatusBadgeClass(active: boolean): string {
  return active
    ? 'bg-green-100 text-green-700'
    : 'bg-red-100 text-red-700';
}
