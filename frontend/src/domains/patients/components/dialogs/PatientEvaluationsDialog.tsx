import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Eye } from 'lucide-react';
import Dialog from '@/shared/components/dialog/Dialog';
import type { Evaluation, Patient } from '@/types';
import { getPatientEvaluations } from '../../api';

interface PatientEvaluationsDialogProps {
  patient: Patient | null;
  open: boolean;
  onClose: () => void;
}

function getScoreBadgeClass(score: number): string {
  if (score <= 29.5) {
    return 'bg-green-100 text-green-700';
  }

  if (score < 37) {
    return 'bg-amber-100 text-amber-700';
  }

  return 'bg-red-100 text-red-700';
}

export default function PatientEvaluationsDialog({
  patient,
  open,
  onClose,
}: PatientEvaluationsDialogProps) {
  const navigate = useNavigate();
  const [evaluations, setEvaluations] = useState<Evaluation[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!open || !patient) {
      setEvaluations([]);
      setLoading(false);
      setError('');
      return;
    }

    let active = true;

    const load = async () => {
      setLoading(true);
      setError('');

      try {
        const data = await getPatientEvaluations(patient.id);
        if (active) {
          setEvaluations(data);
        }
      } catch (err: unknown) {
        if (active) {
          setError(err instanceof Error ? err.message : 'Erro ao carregar avaliacoes do paciente');
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    };

    load();

    return () => {
      active = false;
    };
  }, [open, patient]);

  return (
    <Dialog
      isOpen={open}
      onClose={onClose}
      title="Avaliacoes do paciente"
      description={
        patient
          ? `Historico de avaliacoes vinculadas a ${patient.nome}.`
          : 'Historico de avaliacoes vinculadas ao paciente.'
      }
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
      {loading ? (
        <div className="rounded-xl border border-gray-200 bg-gray-50 px-4 py-8 text-center text-sm text-gray-500">
          Carregando avaliacoes...
        </div>
      ) : error ? (
        <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      ) : evaluations.length === 0 ? (
        <div className="rounded-xl border border-dashed border-gray-300 bg-gray-50 px-4 py-8 text-center text-sm text-gray-500">
          Nenhuma avaliacao vinculada a este paciente.
        </div>
      ) : (
        <div className="space-y-3">
          {evaluations.map((evaluation) => (
            <div key={evaluation.id} className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
              <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                <div>
                  <p className="text-sm font-semibold text-gray-900">{evaluation.classificacao}</p>
                  <p className="text-xs text-gray-500">
                    Avaliador: {evaluation.avaliadorNome || 'Nao informado'}
                  </p>
                  <p className="mt-1 text-xs text-gray-500">
                    {new Date(evaluation.dataAvaliacao).toLocaleDateString('pt-BR')}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <span
                    className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-bold ${getScoreBadgeClass(evaluation.scoreTotal)}`}
                  >
                    {evaluation.scoreTotal}/60
                  </span>
                  <button
                    type="button"
                    onClick={() => { onClose(); navigate(`/avaliacoes/${evaluation.id}`); }}
                    className="inline-flex items-center gap-1 rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 transition hover:bg-gray-50"
                  >
                    <Eye className="h-3.5 w-3.5" />
                    Visualizar
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </Dialog>
  );
}
