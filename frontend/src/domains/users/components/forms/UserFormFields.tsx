import { useEffect } from 'react';
import type { UserRole } from '@/types';
import type { Group } from '@/domains/groups/types';
import { roleOptions } from '../utils/userUtils';

export interface UserFormValues {
  nome: string;
  email: string;
  confirmEmail: string;
  role: UserRole;
  groupIds: string[];
}

interface UserFormFieldsProps {
  values: UserFormValues;
  onChange: (field: Exclude<keyof UserFormValues, 'groupIds'>, value: string) => void;
  onGroupIdsChange?: (groupIds: string[]) => void;
  groups?: Group[];
  singleGroupSelect?: boolean;
  excludeRoles?: UserRole[];
  disabled?: boolean;
  emailHint?: string;
}

export default function UserFormFields({
  values,
  onChange,
  onGroupIdsChange,
  groups = [],
  singleGroupSelect = false,
  excludeRoles = [],
  disabled = false,
  emailHint,
}: UserFormFieldsProps) {
  const showGroups = values.role !== 'analista' && groups.length > 0;
  const showSingleGroupSelect = showGroups && singleGroupSelect && (groups.length > 1 || values.role === 'gestor');

  useEffect(() => {
    if (singleGroupSelect && groups.length === 1 && onGroupIdsChange && values.groupIds.length === 0) {
      onGroupIdsChange([groups[0].id]);
    }
  }, [groups, singleGroupSelect]);

  const toggleGroup = (groupId: string) => {
    if (!onGroupIdsChange) {
      return;
    }

    const nextGroupIds = values.groupIds.includes(groupId)
      ? values.groupIds.filter((id) => id !== groupId)
      : [...values.groupIds, groupId];
    onGroupIdsChange(nextGroupIds);
  };

  const handleSingleGroupChange = (groupId: string) => {
    if (!onGroupIdsChange) return;
    onGroupIdsChange(groupId ? [groupId] : []);
  };

  return (
    <div className="space-y-4">
      <div className="grid gap-4 md:grid-cols-2">
        <div className="md:col-span-2">
          <label className="mb-1 block text-sm font-medium">Nome</label>
          <input
            value={values.nome}
            onChange={(event) => onChange('nome', event.target.value)}
            required
            disabled={disabled}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium">E-mail</label>
          <input
            type="email"
            value={values.email}
            onChange={(event) => onChange('email', event.target.value)}
            required
            disabled={disabled}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          />
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium">Confirmar e-mail</label>
          <input
            type="email"
            value={values.confirmEmail}
            onChange={(event) => onChange('confirmEmail', event.target.value)}
            required
            disabled={disabled}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          />
        </div>

        <div className="md:col-span-2">
          <label className="mb-1 block text-sm font-medium">Perfil</label>
          <select
            value={values.role}
            onChange={(event) => onChange('role', event.target.value)}
            disabled={disabled}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          >
            {roleOptions.filter((o) => !excludeRoles.includes(o.value)).map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      {showSingleGroupSelect && (
        <div>
          <label className="mb-1 block text-sm font-medium">
            Grupo vinculado <span className="text-gray-400 font-normal">(opcional)</span>
          </label>
          <select
            value={values.groupIds[0] ? String(values.groupIds[0]) : ''}
            onChange={(event) => handleSingleGroupChange(event.target.value)}
            disabled={disabled}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          >
            <option value="">Nenhum grupo adicional</option>
            {groups.map((group) => (
              <option key={group.id} value={String(group.id)}>
                {group.nome}
              </option>
            ))}
          </select>
        </div>
      )}

      {showGroups && !singleGroupSelect && (
        <div>
          <label className="mb-2 block text-sm font-medium">Grupos vinculados</label>
          <div className="grid gap-2 rounded-lg border border-gray-200 p-3 md:grid-cols-2">
            {groups.map((group) => (
              <label key={group.id} className="flex items-center gap-2 text-sm text-gray-700">
                <input
                  type="checkbox"
                  checked={values.groupIds.includes(group.id)}
                  onChange={() => toggleGroup(group.id)}
                  disabled={disabled}
                  className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500 disabled:cursor-not-allowed"
                />
                <span>{group.nome}</span>
              </label>
            ))}
          </div>
        </div>
      )}

      {emailHint && <div className="text-xs text-gray-500">{emailHint}</div>}
    </div>
  );
}
