import { useEffect, useState } from 'react';
import { createForm, getGrupos } from '../api';
import type { FormQuestion, Grupo } from '../types';
import { Plus, Trash2, Save } from 'lucide-react';
import Dialog from '@/shared/components/dialog/Dialog';
import { useAuthStore } from '@/shared/store/authStore';

function emptyQuestion(): FormQuestion {
  return { texto: '', peso: 1, ordem: 1, ativa: true };
}

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onCreated: () => void;
}

export default function FormCreateDialog({ isOpen, onClose, onCreated }: Props) {
  const isAdmin = useAuthStore((s) => s.isAdmin());
  const [nome, setNome] = useState('');
  const [descricao, setDescricao] = useState('');
  const [groupId, setGroupId] = useState<number | ''>('');
  const [perguntas, setPerguntas] = useState<FormQuestion[]>([emptyQuestion()]);
  const [grupos, setGrupos] = useState<Grupo[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!isOpen) return;
    getGrupos().then((lista) => {
      setGrupos(lista);
      if (!isAdmin && lista.length === 1) {
        setGroupId(lista[0].id);
      }
    }).catch(() => {});
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) {
      setNome('');
      setDescricao('');
      setGroupId('');
      setPerguntas([emptyQuestion()]);
      setError('');
    }
  }, [isOpen]);

  const pesoTotal = perguntas.reduce((sum, q) => sum + (Number(q.peso) || 0), 0);

  function addPergunta() {
    setPerguntas((prev) => [...prev, { ...emptyQuestion(), ordem: prev.length + 1 }]);
  }

  function removePergunta(index: number) {
    if (perguntas.length <= 1) return;
    setPerguntas((prev) =>
      prev.filter((_, i) => i !== index).map((q, i) => ({ ...q, ordem: i + 1 }))
    );
  }

  function updatePergunta(index: number, field: keyof FormQuestion, value: string | number) {
    setPerguntas((prev) => {
      const updated = [...prev];
      updated[index] = { ...updated[index], [field]: value };
      return updated;
    });
  }

  function movePergunta(index: number, direction: -1 | 1) {
    const newIndex = index + direction;
    if (newIndex < 0 || newIndex >= perguntas.length) return;
    setPerguntas((prev) => {
      const arr = [...prev];
      [arr[index], arr[newIndex]] = [arr[newIndex], arr[index]];
      return arr.map((q, i) => ({ ...q, ordem: i + 1 }));
    });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError('');

    const validPerguntas = perguntas.filter(
      (q) => q.texto.trim().length > 0 && Number(q.peso) > 0
    );
    if (validPerguntas.length === 0) {
      setError('Adicione pelo menos uma pergunta válida com texto e peso.');
      return;
    }

    setSaving(true);
    try {
      await createForm({
        nome,
        descricao: descricao || undefined,
        groupId: groupId ? Number(groupId) : undefined,
        perguntas: validPerguntas.map((q, i) => ({
          texto: q.texto.trim(),
          peso: Number(q.peso),
          ordem: i + 1,
        })),
      });
      onCreated();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar formulário');
    } finally {
      setSaving(false);
    }
  }

  const footer = (
    <>
      <button
        type="button"
        onClick={onClose}
        className="px-4 py-2 text-sm text-gray-600 hover:text-gray-800 font-medium"
      >
        Cancelar
      </button>
      <button
        type="submit"
        form="form-create-form"
        disabled={saving}
        className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 text-sm font-medium"
      >
        <Save className="w-4 h-4" />
        {saving ? 'Salvando...' : 'Salvar Formulário'}
      </button>
    </>
  );

  return (
    <Dialog
      isOpen={isOpen}
      title="Novo Formulário"
      description="Crie um novo formulário com perguntas e pesos"
      onClose={onClose}
      closeDisabled={saving}
      size="lg"
      footer={footer}
    >
      <form id="form-create-form" onSubmit={handleSubmit} className="space-y-5 max-h-[60vh] overflow-y-auto pr-1">
        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
            {error}
          </div>
        )}

        {/* Informações */}
        <div className="space-y-3">
          <h3 className="text-sm font-semibold text-gray-700">Informações</h3>
          <div>
            <label className="block text-sm font-medium mb-1">Nome *</label>
            <input
              value={nome}
              onChange={(e) => setNome(e.target.value)}
              required
              maxLength={200}
              placeholder="Nome do formulário"
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-sm"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Descrição</label>
            <textarea
              value={descricao}
              onChange={(e) => setDescricao(e.target.value)}
              maxLength={1000}
              rows={2}
              placeholder="Descrição opcional..."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none resize-none text-sm"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Grupo</label>
            {!isAdmin && grupos.length === 1 ? (
              <input
                value={grupos[0].nome}
                disabled
                className="w-full px-3 py-2 border border-gray-200 rounded-lg bg-gray-100 text-sm text-gray-600 cursor-not-allowed"
              />
            ) : (
              <select
                value={groupId}
                onChange={(e) => setGroupId(e.target.value ? Number(e.target.value) : '')}
                required={!isAdmin}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-sm"
              >
                {isAdmin && <option value="">Todos os grupos</option>}
                {!isAdmin && <option value="">Selecione um grupo</option>}
                {grupos.map((g) => (
                  <option key={g.id} value={g.id}>{g.nome}</option>
                ))}
              </select>
            )}
          </div>
        </div>

        {/* Perguntas */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-gray-700">
              Perguntas ({perguntas.length}) — Peso total: {pesoTotal}
            </h3>
            <button
              type="button"
              onClick={addPergunta}
              className="flex items-center gap-1 text-xs text-blue-600 hover:text-blue-800 font-medium"
            >
              <Plus className="w-3 h-3" />
              Adicionar pergunta
            </button>
          </div>

          <div className="space-y-2">
            {perguntas.map((q, i) => (
              <div
                key={i}
                className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg border border-gray-200"
              >
                <span className="text-xs font-bold text-gray-400 pt-2 w-6 text-center shrink-0">
                  #{i + 1}
                </span>
                <div className="flex-1 grid grid-cols-1 md:grid-cols-3 gap-2">
                  <div className="md:col-span-2">
                    <input
                      value={q.texto}
                      onChange={(e) => updatePergunta(i, 'texto', e.target.value)}
                      placeholder="Texto da pergunta..."
                      maxLength={1000}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-sm"
                    />
                  </div>
                  <div className="flex items-center gap-2">
                    <label className="text-xs text-gray-500 shrink-0">Peso:</label>
                    <input
                      type="number"
                      value={q.peso}
                      onChange={(e) => updatePergunta(i, 'peso', Number(e.target.value))}
                      min={0.01}
                      step={0.01}
                      className="w-20 px-2 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-sm"
                    />
                    <div className="flex items-center gap-0.5 ml-auto">
                      <button
                        type="button"
                        onClick={() => movePergunta(i, -1)}
                        disabled={i === 0}
                        className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30"
                      >↑</button>
                      <button
                        type="button"
                        onClick={() => movePergunta(i, 1)}
                        disabled={i === perguntas.length - 1}
                        className="p-1 text-gray-400 hover:text-gray-600 disabled:opacity-30"
                      >↓</button>
                      <button
                        type="button"
                        onClick={() => removePergunta(i)}
                        disabled={perguntas.length <= 1}
                        className="p-1 text-red-400 hover:text-red-600 disabled:opacity-30"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </form>
    </Dialog>
  );
}
