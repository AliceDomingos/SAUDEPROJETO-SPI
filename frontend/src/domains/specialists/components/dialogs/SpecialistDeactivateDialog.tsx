import Dialog from '@/shared/components/dialog/Dialog';
import type { Specialist } from '../../types';

interface SpecialistDeactivateDialogProps {
  specialist: Specialist | null;
  open: boolean;
  onClose: () => void;
  onConfirm: (specialistId: string) => Promise<void>;
}

export default function SpecialistDeactivateDialog({
  specialist,
  open,
  onClose,
  onConfirm,
}: SpecialistDeactivateDialogProps) {
  const handleConfirm = async () => {
    if (!specialist) {
      return;
    }

    await onConfirm(specialist.id);
    onClose();
  };

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Desativar especialista"
      description="O especialista deixa de aparecer em novos encaminhamentos, mas o historico permanece preservado."
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={handleConfirm}
            className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-700"
          >
            Desativar
          </button>
        </>
      }
    >
      <p className="text-sm text-gray-700">
        Deseja desativar <span className="font-semibold">{specialist?.nome}</span>?
      </p>
    </Dialog>
  );
}
