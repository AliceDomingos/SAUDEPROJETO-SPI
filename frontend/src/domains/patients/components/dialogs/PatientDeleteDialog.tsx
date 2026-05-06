import { useEffect, useState } from 'react';
import Dialog from '@/shared/components/dialog/Dialog';
import type { Patient } from '@/types';
import { formatCpf } from '../utils/patientUtils';

interface PatientDeleteDialogProps {
  patient: Patient | null;
  open: boolean;
  onClose: () => void;
  onConfirm: (patientId: string) => Promise<void>;
}

export default function PatientDeleteDialog({
  patient,
  open,
  onClose,
  onConfirm,
}: PatientDeleteDialogProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (open) {
      setLoading(false);
      setError('');
    }
  }, [open]);

  const handleConfirm = async () => {
    if (!patient) {
      return;
    }

    setLoading(true);
    setError('');

    try {
      await onConfirm(patient.id);
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao excluir paciente');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Excluir paciente"
      description="Essa acao remove o paciente da lista e deve ser feita com cuidado."
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
            type="button"
            onClick={handleConfirm}
            disabled={loading}
            className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-700 disabled:opacity-50"
          >
            {loading ? 'Excluindo...' : 'Excluir paciente'}
          </button>
        </>
      }
    >
      <div className="space-y-3 text-sm text-gray-600">
        <p>
          Voce esta prestes a excluir <span className="font-semibold text-gray-900">{patient?.nome}</span>.
        </p>
        <p>CPF vinculado: <span className="font-medium text-gray-900">{formatCpf(patient?.cpf)}</span></p>
        <p>Confirme somente se tiver certeza, para evitar exclusao acidental.</p>

        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
            {error}
          </div>
        )}
      </div>
    </Dialog>
  );
}
