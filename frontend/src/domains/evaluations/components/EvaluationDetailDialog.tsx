import { useEffect, useState } from 'react';
import { getEvals } from '@/domains/dashboard/api';
import { getFormById } from '@/domains/forms/api';
import type { Evaluation } from '@/types';
import Dialog from '@/shared/components/dialog/Dialog';
import ScoreChart from './ScoreChart';
import { SPI_QUESTIONS } from '../utils/questions';

interface EvaluationDetailDialogProps {
  evalId: number | null;
  onClose: () => void;
}

export default function EvaluationDetailDialog({ evalId, onClose }: EvaluationDetailDialogProps) {
  const [evalData, setEvalData] = useState<Evaluation | null>(null);
  const [formQuestions, setFormQuestions] = useState<{ id: number; name: string }[] | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (evalId === null) {
      setEvalData(null);
      setFormQuestions(null);
      return;
    }
    setLoading(true);
    getEvals()
      .then((data: Evaluation[]) => {
        const found = data.find((e) => e.id === evalId) ?? null;
        setEvalData(found);
        if (found?.formId) {
          return getFormById(found.formId).then((form) => {
            setFormQuestions(
              form.perguntas
                .sort((a, b) => a.ordem - b.ordem)
                .map((p) => ({ id: p.id!, name: p.texto }))
            );
          }).catch(() => setFormQuestions(null));
        }
      })
      .finally(() => setLoading(false));
  }, [evalId]);

  const score = evalData?.scoreTotal ?? 0;
  const clsBg = score <= 29.5
    ? 'bg-green-100 text-green-700'
    : score < 37
    ? 'bg-amber-100 text-amber-700'
    : 'bg-red-100 text-red-700';
  const scoreColor = score <= 29.5 ? '#16a34a' : score < 37 ? '#ca8a04' : '#dc2626';
  const labels = ['Normal', 'Leve', 'Moderado', 'Grave'];
  const questions = formQuestions ?? SPI_QUESTIONS;

  return (
    <Dialog
      isOpen={evalId !== null}
      onClose={onClose}
      title="Detalhes da Avaliacao"
      description={evalData?.patientNome}
      size="xl"
    >
      {loading && (
        <div className="py-12 text-center text-sm text-gray-400">Carregando...</div>
      )}

      {!loading && evalData && (
        <div className="space-y-5">
          {/* Info */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 rounded-xl border border-gray-200 bg-white p-4 text-sm">
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
              <p className="text-gray-500">Classificacao</p>
              <p className={`inline-block rounded-full px-2 py-0.5 text-xs font-bold ${clsBg}`}>
                {evalData.classificacao}
              </p>
            </div>
          </div>

          {/* Score */}
          <div className="rounded-xl border border-gray-200 bg-white p-8 text-center">
            <p className="mb-2 text-sm text-gray-500">Pontuacao Total (max. {evalData.pesoTotal})</p>
            <p className="text-5xl font-extrabold" style={{ color: scoreColor }}>{score}</p>
            <p className={`mt-3 inline-block rounded-full px-4 py-2 text-sm font-bold ${clsBg}`}>
              {score}/{evalData.pesoTotal} — {evalData.classificacao}
            </p>
          </div>

          {/* Radar */}
          <ScoreChart respostas={evalData.respostas} questions={formQuestions ?? undefined} />

          {/* Breakdown */}
          <div className="rounded-xl border border-gray-200 bg-white p-4">
            <h3 className="mb-3 text-sm font-semibold text-gray-700">Detalhamento por Dimensao</h3>
            <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-4">
              {questions.map((q, idx) => {
                const v = evalData.respostas[q.id] || 0;
                return (
                  <div key={q.id} className="flex items-center gap-2 rounded-lg bg-gray-50 p-2">
                    <span className="w-5 text-xs font-bold text-gray-500">#{idx + 1}</span>
                    <span className={`flex h-6 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white ${
                      v <= 2 ? 'bg-green-500' : v === 3 ? 'bg-amber-500' : 'bg-red-500'
                    }`}>{v}</span>
                    <span className="truncate text-xs" title={q.name}>{q.name}</span>
                    <span className="ml-auto text-xs text-gray-400">{labels[v - 1]}</span>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}
    </Dialog>
  );
}
