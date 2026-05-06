import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getEvals } from '@/domains/dashboard/api';
import { getFormById } from '@/domains/forms/api';
import type { Evaluation } from '@/types';
import ScoreChart from '../components/ScoreChart';
import { SPI_QUESTIONS } from '../utils/questions';
import { ArrowLeft } from 'lucide-react';
import EvaluationReferralDecision from '../components/EvaluationReferralDecision';

export default function EvaluationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [evalData, setEvalData] = useState<Evaluation | null>(null);
  const [formQuestions, setFormQuestions] = useState<{ id: number; name: string }[] | null>(null);

  useEffect(() => {
    getEvals().then((data: Evaluation[]) => {
      const found = data.find((e: Evaluation) => e.id === id);
      setEvalData(found || null);
      if (found?.formId) {
        getFormById(found.formId).then((form) => {
          const qs = form.perguntas
            .sort((a, b) => a.ordem - b.ordem)
            .map((p) => ({ id: p.id!, name: p.texto }));
          setFormQuestions(qs);
        }).catch(() => setFormQuestions(null));
      }
    });
  }, [id]);

  if (!evalData) {
    return <div className="text-center py-8 text-gray-400">Carregando...</div>;
  }

  const score = evalData.scoreTotal;
  const clsBg = score <= 29.5
    ? 'bg-green-100 text-green-700'
    : score < 37
    ? 'bg-amber-100 text-amber-700'
    : 'bg-red-100 text-red-700';

  const scoreColor = score <= 29.5 ? '#16a34a' : score < 37 ? '#ca8a04' : '#dc2626';

  const labels = ['Normal', 'Leve', 'Moderado', 'Grave'];

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <button onClick={() => navigate('/avaliacoes')} className="p-1 hover:bg-gray-100 rounded">
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div>
          <h2 className="text-xl font-bold">Detalhes da Avaliação</h2>
          <p className="text-sm text-gray-500">{evalData.patientNome}</p>
        </div>
      </div>

      {/* Info Card */}
      <div className="bg-white rounded-xl border border-gray-200 p-4 grid grid-cols-2 sm:grid-cols-4 gap-3 text-sm">
        <div>
          <p className="text-gray-500">Paciente</p>
          <p className="font-medium">{evalData.patientNome}</p>
        </div>
        <div>
          <p className="text-gray-500">Avaliador</p>
          <p className="font-medium">{evalData.avaliadorNome}</p>
        </div>
        <div>
          <p className="text-gray-500">Data</p>
          <p className="font-medium">{new Date(evalData.dataAvaliacao).toLocaleDateString('pt-BR')}</p>
        </div>
        <div>
          <p className="text-gray-500">Classificação</p>
          <p className={`font-bold ${clsBg} px-2 py-0.5 rounded-full text-xs inline-block`}>{evalData.classificacao}</p>
        </div>
      </div>

      {/* Score */}
      <div className="bg-white rounded-xl border border-gray-200 p-8 text-center">
        <p className="text-sm text-gray-500 mb-2">Pontuação Total (máx. {evalData.pesoTotal})</p>
        <p className="text-5xl font-extrabold" style={{ color: scoreColor }}>
          {score}
        </p>
        <p className={`inline-block px-4 py-2 rounded-full font-bold text-sm mt-3 ${clsBg}`}>
          {score}/{evalData.pesoTotal} — {evalData.classificacao}
        </p>
      </div>

      {/* Radar */}
      <ScoreChart respostas={evalData.respostas} questions={formQuestions ?? undefined} />

      <EvaluationReferralDecision
        evaluationId={evalData.id}
        currentReferral={evalData.referral}
        onSaved={(referral) => setEvalData((current) => (current ? { ...current, referral } : current))}
      />

      {evalData.observacoes ? (
        <div className="bg-white rounded-xl border border-gray-200 p-4">
          <h3 className="text-sm font-semibold text-gray-700 mb-2">Observacao do avaliador</h3>
          <p className="whitespace-pre-wrap text-sm text-gray-700">{evalData.observacoes}</p>
        </div>
      ) : null}

      {/* Breakdown */}
      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <h3 className="text-sm font-semibold text-gray-700 mb-3">Detalhamento por Dimensão</h3>
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-2">
          {(formQuestions ?? SPI_QUESTIONS).map((q) => {
            const v = evalData.respostas[q.id] || 0;
            return (
              <div key={q.id} className="flex items-center gap-2 p-2 bg-gray-50 rounded-lg">
                <span className="text-xs font-bold text-gray-500 w-5">#{q.id}</span>
                <span className={`text-xs font-bold text-white w-8 h-6 rounded-full flex items-center justify-center flex-shrink-0 ${
                  v <= 2 ? 'bg-green-500' : v === 3 ? 'bg-amber-500' : 'bg-red-500'
                }`}>{v}</span>
                <span className="text-xs truncate" title={q.name}>{q.name}</span>
                <span className="text-xs text-gray-400 ml-auto">{labels[v - 1]}</span>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}



