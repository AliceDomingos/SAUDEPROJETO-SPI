import { useEffect, useState } from 'react';
import type { User } from '@/types';
import Dialog from '@/shared/components/dialog/Dialog';
import type { Group } from '../../types';
import type { UpdateGroupInput } from '../../api';
import type { GroupFormValues } from '../../types';
import GroupFormFields from '../forms/GroupFormFields';
import {
  buildGroupFormValues,
  mapGroupFormToInput,
  validateGroupForm,
} from '../utils/groupUtils';

interface GroupEditDialogProps {
  group: Group | null;
  open: boolean;
  onClose: () => void;
  onSubmit: (groupId: string, data: UpdateGroupInput) => Promise<void>;
  managers: User[];
  requireManager: boolean;
}

export default function GroupEditDialog({
  group,
  open,
  onClose,
  onSubmit,
  managers,
  requireManager,
}: GroupEditDialogProps) {
  const [values, setValues] = useState<GroupFormValues>(buildGroupFormValues(group));
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (open) {
      setValues(buildGroupFormValues(group));
      setLoading(false);
      setError('');
    }
  }, [open, group]);

  const handleChange = (field: keyof GroupFormValues, value: string) => {
    setValues((current) => ({ ...current, [field]: value }));
  };

  const handleSubmit = async (event: { preventDefault: () => void }) => {
    event.preventDefault();

    if (!group) {
      return;
    }

    setLoading(true);
    setError('');

    try {
      const validationError = validateGroupForm(values, managers, requireManager);
      if (validationError) {
        throw new Error(validationError);
      }

      await onSubmit(group.id, mapGroupFormToInput(values));
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao editar grupo');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Editar grupo"
      description="Atualize os dados principais do grupo selecionado."
      size="lg"
      closeDisabled={loading}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="submit"
            form="edit-group-form"
            disabled={loading}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:opacity-50"
          >
            {loading ? 'Salvando...' : 'Salvar alteracoes'}
          </button>
        </>
      }
    >
      <form id="edit-group-form" onSubmit={handleSubmit} className="space-y-4">
        <GroupFormFields
          values={values}
          onChange={handleChange}
          managers={managers}
          disabled={loading}
          showManagerField={requireManager}
          managerHint={requireManager ? undefined : 'Esse grupo permanece vinculado ao gestor logado.'}
        />

        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
            {error}
          </div>
        )}
      </form>
    </Dialog>
  );
}
