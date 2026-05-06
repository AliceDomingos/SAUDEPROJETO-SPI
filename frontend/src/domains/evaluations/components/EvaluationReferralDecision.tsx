import { useEffect, useState } from 'react';
import { CheckCircle2, Send, XCircle } from 'lucide-react';
import { saveEvaluationReferral } from '@/domains/dashboard/api';
import type { EvaluationReferral } from '@/types';
import { getSpecialists } from '@/domains/specialists/api';
import type { Specialist } from '@/domains/specialists/types';
import { formatCurrency } from '@/domains/specialists/components/utils/specialistUtils';

interface EvaluationReferralDecisionProps {
  evaluationId?: string;
  currentReferral?: EvaluationReferral | null;
  onSaved?: (referral: EvaluationReferral) => void;
}

export default function EvaluationReferralDecision({ evaluationId, currentReferral, onSaved }: EvaluationReferralDecisionProps) {
  const [referral, setReferral] = useState<EvaluationReferral | null>(currentReferral ?? null);
  const [specialists, setSpecialists] = useState<Specialist[]>([]);
  const [selectedSpecialistId, setSelectedSpecialistId] = useState<string | null>(currentReferral?.specialistId ?? null);
  const [loadingSpecialists, setLoadingSpecialists] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadSpecialists = async () => {
      try {
        const data = await getSpecialists({ activeOnly: true });
        setSpecialists(data);
        setSelectedSpecialistId((current) => current ?? data[0]?.id ?? null);
      } catch {
        setError('Nao foi possivel carregar especialistas.');
      } finally {
        setLoadingSpecialists(false);
      }
    };

    loadSpecialists();
  }, []);

  const handleSave = async (encaminhado: boolean) => {
    if (!evaluationId) {
      setError('A avaliacao precisa estar salva para registrar o encaminhamento.');
      return;
    }

    if (encaminhado && !selectedSpecialistId) {
      setError('Cadastre e selecione um especialista antes de encaminhar.');
      return;
    }

    setSaving(true);
    setError('');
    try {
      const saved = await saveEvaluationReferral(evaluationId, {
        encaminhado,
        specialistId: encaminhado ? selectedSpecialistId : null,
      });
      setReferral(saved);
      onSaved?.(saved);
    } catch {
      setError('Nao foi possivel salvar o encaminhamento.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h3 className="text-sm font-extrabold text-gray-900">Conduta apos avaliacao</h3>
          <p className="mt-1 text-xs font-medium text-gray-500">Registre se o paciente sera encaminhado e alimente o dashboard gerencial.</p>
        </div>
        {referral ? (
          <span className={`inline-flex w-fit items-center gap-1 rounded-full px-3 py-1 text-xs font-bold ${referral.encaminhado ? 'bg-blue-100 text-blue-700' : 'bg-emerald-100 text-emerald-700'}`}>
            {referral.encaminhado ? <Send className="h-3.5 w-3.5" /> : <CheckCircle2 className="h-3.5 w-3.5" />}
            {referral.encaminhado ? 'Encaminhado' : 'Sem encaminhamento'}
          </span>
        ) : null}
      </div>

      <div className="mt-4 grid gap-3 lg:grid-cols-[1fr_auto] lg:items-end">
        <div>
          <p className="mb-2 text-xs font-bold uppercase tracking-wide text-gray-500">Especialista</p>
          {loadingSpecialists ? (
            <div className="rounded-lg border border-gray-200 bg-white px-3 py-3 text-sm text-gray-500">
              Carregando especialistas...
            </div>
          ) : specialists.length === 0 ? (
            <div className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-3 text-sm font-medium text-amber-800">
              Nenhum especialista ativo cadastrado.
            </div>
          ) : (
            <div className="grid gap-2 sm:grid-cols-2">
              {specialists.map((specialist) => (
                <button
                  key={specialist.id}
                  type="button"
                  onClick={() => setSelectedSpecialistId(specialist.id)}
                  className={`rounded-lg border px-3 py-2 text-left text-sm font-semibold transition ${
                    selectedSpecialistId === specialist.id ? 'border-blue-500 bg-blue-50 text-blue-700' : 'border-gray-200 bg-white text-gray-700 hover:border-blue-200 hover:bg-blue-50'
                  }`}
                >
                  <span className="block">{specialist.nome}</span>
                  <span className="block text-xs font-medium opacity-80">
                    {specialist.especialidade} - {formatCurrency(specialist.custoConsulta)}
                  </span>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="flex flex-col gap-2 sm:flex-row lg:flex-col">
          <button
            type="button"
            onClick={() => handleSave(true)}
            disabled={saving || loadingSpecialists || specialists.length === 0}
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-bold text-white transition hover:bg-blue-700 disabled:opacity-50"
          >
            <Send className="h-4 w-4" />
            Encaminhar
          </button>
          <button
            type="button"
            onClick={() => handleSave(false)}
            disabled={saving}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-gray-300 px-4 py-2 text-sm font-bold text-gray-700 transition hover:bg-gray-50 disabled:opacity-50"
          >
            <XCircle className="h-4 w-4" />
            Nao encaminhar
          </button>
        </div>
      </div>

      {referral?.encaminhado ? (
        <div className="mt-4 rounded-lg bg-blue-50 px-3 py-2 text-sm font-semibold text-blue-800">
          {referral.specialistNome ?? referral.especialidade} registrado com custo estimado de {formatCurrency(referral.custoEstimado)}.
        </div>
      ) : null}
      {referral && !referral.encaminhado ? <div className="mt-4 rounded-lg bg-emerald-50 px-3 py-2 text-sm font-semibold text-emerald-800">Avaliacao registrada sem encaminhamento.</div> : null}
      {error ? <div className="mt-4 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm font-semibold text-red-600">{error}</div> : null}
    </div>
  );
}
