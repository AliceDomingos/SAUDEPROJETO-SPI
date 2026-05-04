import Dialog from '@/shared/components/dialog/Dialog';
import type { Formulario } from '../types';

interface FormDetailsDialogProps {
  form: Formulario | null;
  open: boolean;
  onClose: () => void;
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-gray-200 bg-gray-50 px-4 py-3">
      <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">{label}</p>
      <div className="mt-1 text-sm text-gray-800">{value}</div>
    </div>
  );
}

export default function FormDetailsDialog({ form, open, onClose }: FormDetailsDialogProps) {
  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Detalhes do formulario"
      description="Confira os dados principais do formulario sem editar as informacoes."
      size="lg"
      footer={
        <button
          type="button"
          onClick={onClose}
          className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50"
        >
          Fechar
        </button>
      }
    >
      {form && (
        <div className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <InfoRow label="Nome" value={form.nome} />
            <InfoRow label="Grupo" value={form.groupNome || 'Sem grupo'} />
            <InfoRow label="Criado por" value={form.criadoPorNome} />
            <InfoRow label="Status" value={form.ativo ? 'Ativo' : 'Inativo'} />
            <InfoRow label="Perguntas" value={String(form.perguntas.length)} />
            <InfoRow label="Peso total" value={String(form.pesoTotal)} />
          </div>

          {form.descricao && (
            <div className="rounded-xl border border-gray-200 bg-gray-50 px-4 py-3">
              <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">Descricao</p>
              <p className="mt-1 text-sm text-gray-800">{form.descricao}</p>
            </div>
          )}

          {form.perguntas.length > 0 && (
            <div className="rounded-xl border border-gray-200 bg-gray-50 px-4 py-3">
              <p className="mb-3 text-xs font-semibold uppercase tracking-wide text-gray-500">
                Perguntas ({form.perguntas.length})
              </p>
              <ol className="space-y-2">
                {form.perguntas.map((q, i) => (
                  <li key={q.id ?? i} className="flex items-start gap-3 text-sm text-gray-800">
                    <span className="shrink-0 font-semibold text-gray-400">{i + 1}.</span>
                    <span className="flex-1">{q.texto}</span>
                    <span className="shrink-0 text-xs text-gray-400">peso {q.peso}</span>
                  </li>
                ))}
              </ol>
            </div>
          )}
        </div>
      )}
    </Dialog>
  );
}
