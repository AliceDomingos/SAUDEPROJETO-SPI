import { useState, useEffect } from 'react';
import { getClassification } from '../utils/scoring';
import type { EvaluationAnswers, Question } from '../types';
import QuestionCard from '../components/QuestionCard';
import ScoreChart from '../components/ScoreChart';
import { createEvaluation } from '@/domains/dashboard/api';
import { getForms } from '@/domains/forms/api';
import type { Formulario } from '@/domains/forms/types';
import Dialog from '@/shared/components/dialog/Dialog';

interface EvaluationFormPageProps {
  embedded?: boolean;
  onCancel?: () => void;
}

interface ResultData {
  score: number;
  pesoTotal: number;
  classification: string;
  color: string;
  cls: string;
  answers: Record<number, number>;
  questions: { id: number; name: string }[];
}

export default function EvaluationFormPage({ embedded = false, onCancel }: EvaluationFormPageProps) {
  const [formularios, setFormularios] = useState<Formulario[]>([]);
  const [selectedFormId, setSelectedFormId] = useState<number | null>(null);
  const [activeQuestions, setActiveQuestions] = useState<Question[]>([]);

  const [mode, setMode] = useState<'existing' | 'new'>('existing');
  const [existingPatientId, setExistingPatientId] = useState<number | null>(null);
  const [newName, setNewName] = useState('');
  const [answers, setAnswers] = useState<EvaluationAnswers>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [resultData, setResultData] = useState<ResultData | null>(null);

  useEffect(() => {
    getForms().catch(() => []).then(setFormularios);
  }, []);

  function selectForm(formId: number) {
    setSelectedFormId(formId);
    setAnswers({});
    setError('');

    const form = formularios.find((f) => f.id === formId);
    if (form) {
      const questions: Question[] = form.perguntas
        .sort((a, b) => a.ordem - b.ordem)
        .map((p, idx) => {
          const maxScore = Math.max(2, Math.round(p.peso));
          return {
            id: p.id ?? idx + 1,
            ordem: idx + 1,
            name: p.texto,
            options: Array.from({ length: maxScore }, (_, i) => ({
              score: i + 1,
              text: i === 0 ? 'Nao apresenta' : i === maxScore - 1 ? 'Sempre apresenta' : `Nivel ${i + 1}`,
            })),
          };
        });
      setActiveQuestions(questions);
    }
  }

  function closeResult() {
    setResultData(null);
    onCancel?.();
  }

  const total = activeQuestions.length;
  const answered = Object.keys(answers).length;
  const progress = total > 0 ? (answered / total) * 100 : 0;

  const handleSubmit = async () => {
    if (mode === 'existing' && !existingPatientId) {
      setError('Selecione um paciente existente.');
      return;
    }
    if (mode === 'new' && !newName.trim()) {
      setError('Informe o nome do paciente.');
      return;
    }
    if (answered < total) {
      setError(`Faltam ${total - answered} questao(oes) para responder.`);
      return;
    }

    setError('');
    setLoading(true);
    try {
      const pid = mode === 'existing' ? existingPatientId! : Date.now();
      const created = await createEvaluation({ patientId: pid, respostas: answers, formId: selectedFormId! });
      const score = Number(created.scoreTotal);
      const pesoTotal = Number(created.pesoTotal);
      const form = formularios.find((f) => f.id === selectedFormId);
      const faixas = [...(form?.faixas ?? [])].sort((a, b) => a.scoreMin - b.scoreMin);
      let classification: string;
      let color: string;
      let cls: string;
      if (faixas.length > 0) {
        const idx = faixas.findIndex((f) => score >= f.scoreMin && score <= f.scoreMax);
        const pos = idx === -1 ? faixas.length - 1 : idx;
        classification = faixas[pos]?.rotulo ?? created.classificacao;
        if (pos === 0) { color = '#16a34a'; cls = 'not-tea'; }
        else if (pos === faixas.length - 1) { color = '#dc2626'; cls = 'tea-grave'; }
        else { color = '#ca8a04'; cls = 'tea-leve'; }
      } else {
        const result = getClassification(score);
        classification = result.classification;
        color = result.color;
        cls = result.cls;
      }
      const questions = activeQuestions.map((q) => ({ id: q.id, name: q.name }));
      setResultData({ score, pesoTotal, classification, color, cls, answers, questions });
    } catch {
      setError('Erro ao salvar avaliacao');
    } finally {
      setLoading(false);
    }
  };

  // Passo 1 — selecionar formulario
  if (selectedFormId === null) {
    return (
      <div className={embedded ? 'space-y-4' : 'mx-auto max-w-3xl space-y-5'}>
        {!embedded && (
          <div>
            <h2 className="text-xl font-bold">Nova Avaliacao</h2>
            <p className="text-sm text-gray-500">Selecione o formulario a ser utilizado</p>
          </div>
        )}

        <div className="rounded-xl border border-gray-200 bg-white p-5 space-y-3">
          <h3 className="text-sm font-semibold text-gray-700">Formulario de avaliacao</h3>

          {formularios.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-4">
              Nenhum formulario disponivel. Importe um formulario na secao Formularios.
            </p>
          ) : (
            <div className="space-y-2">
              {formularios.map((f) => (
                <button
                  key={f.id}
                  type="button"
                  onClick={() => selectForm(f.id)}
                  className="w-full rounded-lg border border-gray-200 bg-white px-4 py-3 text-left transition hover:border-blue-300 hover:bg-blue-50"
                >
                  <p className="text-sm font-semibold text-gray-800">{f.nome}</p>
                  <p className="text-xs text-gray-400 mt-0.5">
                    {f.perguntas.length} questoes
                    {f.groupNome ? ` · ${f.groupNome}` : ''}
                    {f.descricao ? ` · ${f.descricao}` : ''}
                  </p>
                </button>
              ))}
            </div>
          )}
        </div>

        {embedded && onCancel && (
          <div className="flex justify-end">
            <button
              type="button"
              onClick={onCancel}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancelar
            </button>
          </div>
        )}
      </div>
    );
  }

  const clsBg = resultData?.cls === 'not-tea'
    ? 'bg-green-100 text-green-700'
    : resultData?.cls === 'tea-leve'
      ? 'bg-amber-100 text-amber-700'
      : 'bg-red-100 text-red-700';

  // Passo 2 — paciente + perguntas juntos
  return (
    <>
      <Dialog
        isOpen={resultData !== null}
        onClose={closeResult}
        title="Resultado da Avaliacao"
        size="xl"
      >
        {resultData && (
          <div className="space-y-5">
            <div className="rounded-xl border border-gray-200 bg-white p-8 text-center">
              <p className="mb-2 text-sm text-gray-500">Pontuacao Total (max. {resultData.pesoTotal})</p>
              <p className="text-5xl font-extrabold" style={{ color: resultData.color }}>
                {resultData.score}
              </p>
              <p className={`mt-3 inline-block rounded-full px-4 py-2 text-sm font-bold ${clsBg}`}>
                {resultData.score}/{resultData.pesoTotal} — {resultData.classification}
              </p>
            </div>

            <ScoreChart respostas={resultData.answers} questions={resultData.questions} />

            <div className="rounded-xl border border-gray-200 bg-white p-4">
              <h3 className="mb-3 text-sm font-semibold text-gray-700">Detalhamento por Dimensao</h3>
              <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-4">
                {resultData.questions.map((q, idx) => {
                  const v = resultData.answers[q.id] || 0;
                  const labels = ['Normal', 'Leve', 'Moderado', 'Grave'];
                  return (
                    <div key={q.id} className="flex items-center gap-2 rounded-lg bg-gray-50 p-2">
                      <span className="w-5 text-xs font-bold text-gray-500">#{idx + 1}</span>
                      <span className={`flex h-6 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white ${
                        v <= 2 ? 'bg-green-500' : v === 3 ? 'bg-amber-500' : 'bg-red-500'
                      }`}>
                        {v}
                      </span>
                      <span className="truncate text-xs" title={q.name}>{q.name}</span>
                      <span className="ml-auto text-xs text-gray-400">{labels[v - 1]}</span>
                    </div>
                  );
                })}
              </div>
            </div>

            <div className="flex justify-end">
              <button
                type="button"
                onClick={closeResult}
                className="rounded-lg bg-blue-600 px-6 py-2 text-sm font-medium text-white hover:bg-blue-700"
              >
                Fechar
              </button>
            </div>
          </div>
        )}
      </Dialog>

      <div className={embedded ? 'space-y-5' : 'mx-auto max-w-3xl space-y-5'}>
        {/* Cabecalho */}
        <div className="flex items-center justify-between">
          <div>
            {!embedded && <h2 className="text-xl font-bold">Nova Avaliacao</h2>}
            <p className="text-sm text-gray-500">
              {formularios.find((f) => f.id === selectedFormId)?.nome}
              {' '}·{' '}
              <button
                type="button"
                onClick={() => { setSelectedFormId(null); setAnswers({}); setError(''); }}
                className="text-blue-600 hover:underline"
              >
                trocar formulario
              </button>
            </p>
          </div>
          <div className="text-sm font-medium text-gray-500">{answered}/{total}</div>
        </div>

        {/* Barra de progresso */}
        <div className="h-2 w-full rounded-full bg-gray-200">
          <div
            className="h-2 rounded-full bg-blue-600 transition-all duration-300"
            style={{ width: `${progress}%` }}
          />
        </div>

        {/* Paciente */}
        <div className="space-y-4 rounded-xl border border-gray-200 bg-white p-5">
          <h3 className="text-sm font-semibold text-gray-700">Paciente</h3>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={() => setMode('existing')}
              className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${mode === 'existing' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600'}`}
            >
              Paciente Existente
            </button>
            <button
              type="button"
              onClick={() => setMode('new')}
              className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${mode === 'new' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600'}`}
            >
              Novo Paciente
            </button>
          </div>
          {mode === 'existing' ? (
            <ExistingPatientSelector value={existingPatientId} onChange={setExistingPatientId} />
          ) : (
            <input
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Nome completo do paciente"
            />
          )}
        </div>

        {/* Perguntas */}
        <div className="space-y-3">
          {activeQuestions.map((q) => (
            <QuestionCard
              key={q.id}
              question={q}
              value={answers[q.id] as number | undefined}
              onChange={(score) => setAnswers((prev) => ({ ...prev, [q.id]: score }))}
            />
          ))}
        </div>

        {error && (
          <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-600">
            {error}
          </div>
        )}

        <div className={`flex ${embedded ? 'justify-end gap-3 pb-2' : 'justify-center pb-8'}`}>
          {embedded && onCancel && (
            <button
              type="button"
              onClick={onCancel}
              disabled={loading}
              className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
            >
              Cancelar
            </button>
          )}
          <button
            type="button"
            onClick={handleSubmit}
            disabled={loading}
            className="rounded-xl bg-green-600 px-8 py-3 text-sm font-semibold text-white shadow-lg shadow-green-600/20 transition hover:bg-green-700 disabled:opacity-50"
          >
            {loading ? 'Salvando...' : 'Salvar Avaliacao'}
          </button>
        </div>
      </div>
    </>
  );
}

function ExistingPatientSelector({ value, onChange }: { value: number | null; onChange: (id: number) => void }) {
  const [patients, setPatients] = useState<{ id: number; nome: string; idade: number | null }[]>([]);
  const [show, setShow] = useState(false);
  const [search, setSearch] = useState('');

  useEffect(() => {
    import('@/domains/patients/api').then(({ getPatients }) => getPatients().then(setPatients));
  }, []);

  const filtered = patients.filter((p) => p.nome.toLowerCase().includes(search.toLowerCase()));
  const selected = patients.find((p) => p.id === value);

  return (
    <div className="relative">
      <input
        value={selected?.nome || search}
        onChange={(e) => { setSearch(e.target.value); setShow(true); }}
        onFocus={() => setShow(true)}
        onBlur={() => setTimeout(() => setShow(false), 200)}
        className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500"
        placeholder="Buscar paciente..."
      />
      {show && (
        <div className="absolute z-10 mt-1 max-h-44 w-full overflow-auto rounded-lg border border-gray-200 bg-white shadow-lg">
          {filtered.length === 0 && (
            <div className="p-3 text-center text-sm text-gray-400">Nenhum paciente encontrado</div>
          )}
          {filtered.map((p) => (
            <button
              key={p.id}
              type="button"
              className={`w-full px-3 py-2 text-left text-sm hover:bg-blue-50 ${p.id === value ? 'bg-blue-50 font-medium text-blue-700' : ''}`}
              onMouseDown={() => { onChange(p.id); setShow(false); setSearch(''); }}
            >
              {p.nome}{p.idade ? ` — ${p.idade} anos` : ''}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
