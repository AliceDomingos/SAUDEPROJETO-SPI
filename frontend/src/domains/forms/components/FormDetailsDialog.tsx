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
              <ol className="space-y-3">
                {form.perguntas.map((q, i) => (
                  <li key={q.id ?? i} className="space-y-1">
                    <div className="flex items-start gap-3 text-sm text-gray-800">
                      <span className="shrink-0 font-semibold text-gray-400">{i + 1}.</span>
                      <span className="flex-1">{q.texto}</span>
                      <span className="shrink-0 text-xs text-gray-400">peso {q.peso}</span>
                    </div>
                    {q.opcoes.length > 0 && (
                      <ul className="ml-6 space-y-0.5">
                        {q.opcoes.map((o) => (
                          <li key={o.valor} className="flex items-center gap-2 text-xs text-gray-500">
                            <span className="flex h-4 w-4 shrink-0 items-center justify-center rounded-full bg-gray-200 font-semibold text-gray-600">
                              {o.valor}
                            </span>
                            <span>{o.descricao}</span>
                          </li>
                        ))}
                      </ul>
                    )}
                  </li>
                ))}
              </ol>
            </div>
          )}

          {form.faixas.length > 0 && (
            <div className="rounded-xl border border-gray-200 bg-gray-50 px-4 py-3">
              <p className="mb-3 text-xs font-semibold uppercase tracking-wide text-gray-500">
                Faixas de Classificação
              </p>
              <div className="space-y-1.5">
                {form.faixas.map((f, i) => (
                  <div key={i} className="flex items-center gap-3 text-sm">
                    <span className="shrink-0 text-xs text-gray-400">
                      {f.scoreMin} — {f.scoreMax}
                    </span>
                    <span className="text-gray-700">{f.rotulo}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </Dialog>
  );
}
