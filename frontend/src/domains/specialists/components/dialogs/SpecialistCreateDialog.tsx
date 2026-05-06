import { useEffect, useState } from 'react';
import Dialog from '@/shared/components/dialog/Dialog';
import type { CreateSpecialistInput } from '../../api';
import type { SpecialistFormValues } from '../../types';
import SpecialistFormFields from '../forms/SpecialistFormFields';
import {
  buildSpecialistFormValues,
  mapSpecialistFormToInput,
  validateSpecialistForm,
} from '../utils/specialistUtils';

interface SpecialistCreateDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateSpecialistInput) => Promise<void>;
}

export default function SpecialistCreateDialog({ open, onClose, onSubmit }: SpecialistCreateDialogProps) {
  const [values, setValues] = useState<SpecialistFormValues>(buildSpecialistFormValues());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (open) {
      setValues(buildSpecialistFormValues());
      setLoading(false);
      setError('');
    }
  }, [open]);

  const handleChange = <K extends keyof SpecialistFormValues>(field: K, value: SpecialistFormValues[K]) => {
    setValues((current) => ({ ...current, [field]: value }));
  };

  const handleSubmit = async (event: { preventDefault: () => void }) => {
    event.preventDefault();
    setLoading(true);
    setError('');

    try {
      const validationError = validateSpecialistForm(values);
      if (validationError) {
        throw new Error(validationError);
      }

      await onSubmit(mapSpecialistFormToInput(values));
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao criar especialista');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Novo especialista"
      description="Cadastre o profissional que podera receber encaminhamentos."
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
            form="create-specialist-form"
            disabled={loading}
            className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-green-700 disabled:opacity-50"
          >
            {loading ? 'Salvando...' : 'Salvar especialista'}
          </button>
        </>
      }
    >
      <form id="create-specialist-form" onSubmit={handleSubmit} className="space-y-4">
        <SpecialistFormFields values={values} onChange={handleChange} disabled={loading} />
        {error && <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">{error}</div>}
      </form>
    </Dialog>
  );
}
