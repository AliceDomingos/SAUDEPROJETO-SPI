import { useEffect, useState } from 'react';
import Dialog from '@/shared/components/dialog/Dialog';
import type { UpdateSpecialistInput } from '../../api';
import type { Specialist, SpecialistFormValues } from '../../types';
import SpecialistFormFields from '../forms/SpecialistFormFields';
import {
  buildSpecialistFormValues,
  mapSpecialistFormToInput,
  validateSpecialistForm,
} from '../utils/specialistUtils';

interface SpecialistEditDialogProps {
  specialist: Specialist | null;
  open: boolean;
  onClose: () => void;
  onSubmit: (specialistId: string, data: UpdateSpecialistInput) => Promise<void>;
}

export default function SpecialistEditDialog({ specialist, open, onClose, onSubmit }: SpecialistEditDialogProps) {
  const [values, setValues] = useState<SpecialistFormValues>(buildSpecialistFormValues(specialist));
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (open) {
      setValues(buildSpecialistFormValues(specialist));
      setLoading(false);
      setError('');
    }
  }, [open, specialist]);

  const handleChange = <K extends keyof SpecialistFormValues>(field: K, value: SpecialistFormValues[K]) => {
    setValues((current) => ({ ...current, [field]: value }));
  };

  const handleSubmit = async (event: { preventDefault: () => void }) => {
    event.preventDefault();
    if (!specialist) {
      return;
    }

    setLoading(true);
    setError('');

    try {
      const validationError = validateSpecialistForm(values);
      if (validationError) {
        throw new Error(validationError);
      }

      await onSubmit(specialist.id, mapSpecialistFormToInput(values));
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao editar especialista');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Editar especialista"
      description="Atualize os dados do profissional e o valor cobrado por consulta."
      size="lg"
      closeDisabled={loading}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="submit"
            form="edit-specialist-form"
            disabled={loading}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:opacity-50"
          >
            {loading ? 'Salvando...' : 'Salvar alteracoes'}
          </button>
        </>
      }
    >
      <form id="edit-specialist-form" onSubmit={handleSubmit} className="space-y-4">
        <SpecialistFormFields values={values} onChange={handleChange} disabled={loading} showStatus />
        {error && <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">{error}</div>}
      </form>
    </Dialog>
  );
}
