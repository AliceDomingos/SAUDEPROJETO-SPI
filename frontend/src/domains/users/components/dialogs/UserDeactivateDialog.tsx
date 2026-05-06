import { useEffect, useState } from 'react';
import Dialog from '@/shared/components/dialog/Dialog';
import type { User } from '@/types';

interface UserDeactivateDialogProps {
  user: User | null;
  open: boolean;
  onClose: () => void;
  onConfirm: (userId: string) => Promise<void>;
}

export default function UserDeactivateDialog({
  user,
  open,
  onClose,
  onConfirm,
}: UserDeactivateDialogProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (open) {
      setLoading(false);
      setError('');
    }
  }, [open]);

  const handleConfirm = async () => {
    if (!user) {
      return;
    }

    setLoading(true);
    setError('');

    try {
      await onConfirm(user.id);
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao desativar usuario');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Desativar usuario"
      description="Essa acao desativa o acesso ao sistema."
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
            {loading ? 'Desativando...' : 'Desativar'}
          </button>
        </>
      }
    >
      <div className="space-y-3 text-sm text-gray-600">
        <p>
          Voce esta prestes a desativar <span className="font-semibold text-gray-900">{user?.nome}</span>.
        </p>
        <p>Depois disso, esse usuario deixa de acessar o sistema ate ser reativado por outro fluxo administrativo.</p>

        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
            {error}
          </div>
        )}
      </div>
    </Dialog>
  );
}
