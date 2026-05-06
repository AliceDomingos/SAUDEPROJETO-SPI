import { useEffect, useState } from 'react';
import Dialog from '@/shared/components/dialog/Dialog';
import type { Group } from '../../types';

interface GroupDeleteDialogProps {
  group: Group | null;
  open: boolean;
  onClose: () => void;
  onConfirm: (groupId: string) => Promise<void>;
}

export default function GroupDeleteDialog({
  group,
  open,
  onClose,
  onConfirm,
}: GroupDeleteDialogProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (open) {
      setLoading(false);
      setError('');
    }
  }, [open]);

  const handleConfirm = async () => {
    if (!group) {
      return;
    }

    setLoading(true);
    setError('');

    try {
      await onConfirm(group.id);
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao excluir grupo');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Excluir grupo"
      description="Essa acao remove o grupo da lista e deve ser feita com cuidado."
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
            {loading ? 'Excluindo...' : 'Excluir grupo'}
          </button>
        </>
      }
    >
      <div className="space-y-3 text-sm text-gray-600">
        <p>
          Voce esta prestes a excluir <span className="font-semibold text-gray-900">{group?.nome}</span>.
        </p>
        <p>Gestor responsavel: <span className="font-medium text-gray-900">{group?.gestor_nome || 'Nao informado'}</span></p>
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
