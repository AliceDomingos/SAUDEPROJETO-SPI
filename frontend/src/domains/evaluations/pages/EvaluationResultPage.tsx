import { useLocation, useNavigate, Link } from 'react-router-dom';
import ScoreChart from '../components/ScoreChart';
import { SPI_QUESTIONS } from '../utils/questions';
import EvaluationReferralDecision from '../components/EvaluationReferralDecision';

interface ResultState {
  score: number;
  pesoTotal: number;
  classification: string;
  color: string;
  cls: string;
  evaluationId?: string;
  patientId: string;
  observacoes?: string | null;
  answers: Record<string, number>;
  questions?: { id: number; name: string }[];
}

export default function EvaluationResultPage() {
  const { state } = useLocation();
  const navigate = useNavigate();

  if (!state || !state.score) {
    navigate('/');
    return null;
  }

  const { score, pesoTotal, classification, color, cls, evaluationId, observacoes,questions, answers }: ResultState = state;
  const labels = ['Normal', 'Leve', 'Moderado', 'Grave'];

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <h2 className="text-xl font-bold">Resultado</h2>

      <div className="rounded-xl border border-gray-200 bg-white p-8 text-center">
        <p className="mb-2 text-sm text-gray-500">Pontuacao Total (max. {pesoTotal})</p>
        <p className="text-5xl font-extrabold" style={{ color }}>
          {score}
        </p>
        <p
          className={`mt-3 inline-block rounded-full px-4 py-2 text-sm font-bold ${
            cls === 'not-tea'
              ? 'bg-green-100 text-green-700'
              : cls === 'tea-leve'
                ? 'bg-amber-100 text-amber-700'
                : 'bg-red-100 text-red-700'
          }`}
        >
          {score}/{pesoTotal} - {classification}
        </p>
      </div>

      <ScoreChart respostas={answers} questions={questions} />

      <EvaluationReferralDecision evaluationId={evaluationId} />

      {observacoes ? (
        <div className="rounded-xl border border-gray-200 bg-white p-4">
          <h3 className="mb-2 text-sm font-semibold text-gray-700">Observacao do avaliador</h3>
          <p className="whitespace-pre-wrap text-sm text-gray-700">{observacoes}</p>
        </div>
      ) : null}

      <div className="rounded-xl border border-gray-200 bg-white p-4">
        <h3 className="mb-3 text-sm font-semibold text-gray-700">Detalhamento por Dimensao</h3>
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-4">
          {(questions ?? SPI_QUESTIONS).map((q) => {
            const v = answers[q.id] || 0;
            return (
              <div key={q.id} className="flex items-center gap-2 rounded-lg bg-gray-50 p-2">
                <span className="w-5 text-xs font-bold text-gray-500">#{q.id}</span>
                <span
                  className={`flex h-6 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white ${
                    v === 1 ? 'bg-green-500' : v === 2 ? 'bg-green-500' : v === 3 ? 'bg-amber-500' : 'bg-red-500'
                  }`}
                >
                  {v}
                </span>
                <span className="truncate text-xs" title={q.name}>{q.name}</span>
                <span className="ml-auto text-xs text-gray-400">{labels[v - 1]}</span>
              </div>
            );
          })}
        </div>
      </div>

      <div className="flex justify-center gap-3">
        <Link
          to="/"
          className="rounded-lg border border-blue-600 px-5 py-2 text-sm font-medium text-blue-600 hover:bg-blue-50"
        >
          Voltar ao Inicio
        </Link>
        <Link
          to="/avaliacoes"
          state={{ openNewEvaluation: true }}
          className="rounded-lg bg-blue-600 px-5 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          Nova Avaliacao
        </Link>
      </div>
    </div>
  );
}



