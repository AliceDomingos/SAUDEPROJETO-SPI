import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { SPI_QUESTIONS } from '../utils/questions';
import { calcScore, getClassification } from '../utils/scoring';
import type { EvaluationAnswers, Question } from '../types';
import QuestionCard from '../components/QuestionCard';
import { createEvaluation } from '@/domains/dashboard/api';
import { getForms } from '@/domains/forms/api';
import type { Formulario } from '@/domains/forms/types';

interface EvaluationFormPageProps {
  embedded?: boolean;
  onCancel?: () => void;
}

const DEFAULT_FORM_ID = 0;

export default function EvaluationFormPage({ embedded = false, onCancel }: EvaluationFormPageProps) {
  const navigate = useNavigate();

  const [formularios, setFormularios] = useState<Formulario[]>([]);
  const [selectedFormId, setSelectedFormId] = useState<number | null>(null);
  const [activeQuestions, setActiveQuestions] = useState<Question[]>([]);
  const [formIdToSend, setFormIdToSend] = useState<number | undefined>(undefined);

  const [mode, setMode] = useState<'existing' | 'new'>('existing');
  const [existingPatientId, setExistingPatientId] = useState<number | null>(null);
  const [newName, setNewName] = useState('');
  const [answers, setAnswers] = useState<EvaluationAnswers>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    getForms().catch(() => []).then(setFormularios);
  }, []);

  function selectForm(formId: number) {
    setSelectedFormId(formId);
    setAnswers({});
    setError('');

    if (formId === DEFAULT_FORM_ID) {
      setActiveQuestions(SPI_QUESTIONS);
      setFormIdToSend(undefined);
    } else {
      const form = formularios.find((f) => f.id === formId);
      if (form) {
        const questions: Question[] = form.perguntas
          .sort((a, b) => a.ordem - b.ordem)
          .map((p, idx) => {
            const maxScore = Math.max(2, Math.round(p.peso));
            return {
              id: p.id ?? idx + 1,
              name: p.texto,
              options: Array.from({ length: maxScore }, (_, i) => ({
                score: i + 1,
                text: i === 0 ? 'Nao apresenta' : i === maxScore - 1 ? 'Sempre apresenta' : `Nivel ${i + 1}`,
              })),
            };
          });
        setActiveQuestions(questions);
        setFormIdToSend(formId);
      }
    }
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
      await createEvaluation({ patientId: pid, respostas: answers, formId: formIdToSend });
      const score = calcScore(answers);
      const classification = getClassification(score);
      navigate('/resultado', {
        state: { ...classification, patientId: pid, patientNome: mode === 'new' ? newName : undefined, answers },
      });
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

          <button
            type="button"
            onClick={() => selectForm(DEFAULT_FORM_ID)}
            className="w-full rounded-lg border-2 border-blue-200 bg-blue-50 px-4 py-3 text-left transition hover:border-blue-400 hover:bg-blue-100"
          >
            <p className="text-sm font-semibold text-blue-700">Formulario Padrao</p>
            <p className="text-xs text-blue-500 mt-0.5">14 questoes — CARS adaptado</p>
          </button>

          {formularios.length > 0 && (
            <div className="space-y-2 pt-1">
              <p className="text-xs font-medium uppercase tracking-wide text-gray-400">Formularios criados</p>
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

  // Passo 2 — paciente + perguntas juntos
  return (
    <div className={embedded ? 'space-y-5' : 'mx-auto max-w-3xl space-y-5'}>
      {/* Cabecalho */}
      <div className="flex items-center justify-between">
        <div>
          {!embedded && <h2 className="text-xl font-bold">Nova Avaliacao</h2>}
          <p className="text-sm text-gray-500">
            {selectedFormId === DEFAULT_FORM_ID
              ? 'Formulario Padrao'
              : formularios.find((f) => f.id === selectedFormId)?.nome}
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
