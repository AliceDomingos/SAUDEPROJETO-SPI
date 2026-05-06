import { useEffect, useState } from 'react';
import Dialog from '@/shared/components/dialog/Dialog';
import type { User } from '@/types';

interface UserPasswordInviteDialogProps {
  user: User | null;
  open: boolean;
  onClose: () => void;
  onConfirm: (userId: string) => Promise<void>;
}

export default function UserPasswordInviteDialog({
  user,
  open,
  onClose,
  onConfirm,
}: UserPasswordInviteDialogProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    if (open) {
      setLoading(false);
      setError('');
      setSuccess('');
    }
  }, [open]);

  const handleConfirm = async () => {
    if (!user) {
      return;
    }

    setLoading(true);
    setError('');
    setSuccess('');

    try {
      await onConfirm(user.id);
      setSuccess('Convite enviado com sucesso.');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Erro ao enviar convite por e-mail');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Enviar convite de senha"
      description="Essa acao enviara um e-mail para o usuario definir a senha de acesso."
      closeDisabled={loading}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            disabled={loading}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Fechar
          </button>
          <button
            type="button"
            onClick={handleConfirm}
            disabled={loading}
            className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-amber-700 disabled:opacity-50"
          >
            {loading ? 'Enviando...' : 'Enviar e-mail'}
          </button>
        </>
      }
    >
      <div className="space-y-3 text-sm text-gray-600">
        <p>
          O convite sera enviado para <span className="font-semibold text-gray-900">{user?.email}</span>.
        </p>
        <p>
          O usuario <span className="font-semibold text-gray-900">{user?.nome}</span> recebera instrucoes para criar a propria senha.
        </p>

        {success && (
          <div className="rounded-lg border border-green-200 bg-green-50 px-3 py-2 text-sm text-green-700">
            {success}
          </div>
        )}

        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
            {error}
          </div>
        )}
      </div>
    </Dialog>
  );
}
