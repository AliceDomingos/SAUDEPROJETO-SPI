import { useState } from 'react';
import { CalendarDays } from 'lucide-react';

export type PeriodSelection = {
  label: string;
  dataInicio: string;
  dataFim: string;
  monthLabel?: string;
};

function formatDateInput(date: string) {
  return date ? new Date(`${date}T00:00:00`).toLocaleDateString('pt-BR') : '-';
}

type PresetKey = 'thisMonth' | 'lastMonth' | 'last30' | 'last3' | 'last6' | 'last12' | 'all';

export default function PeriodFilter({
  value,
  fullStart,
  fullEnd,
  onApply,
  onClear,
}: {
  value: string;
  fullStart: string;
  fullEnd: string;
  onApply: (selection: PeriodSelection) => void;
  onClear: () => void;
}) {
  const [open, setOpen] = useState(false);
  const [customStart, setCustomStart] = useState(fullStart);
  const [customEnd, setCustomEnd] = useState(fullEnd);

  const applyRange = (label: string, start: string, end: string) => {
    onApply({ label, dataInicio: start, dataFim: end });
    setOpen(false);
  };

  const applyPreset = (preset: PresetKey) => {
    if (!fullEnd) return;

    if (preset === 'all') {
      onClear();
      setOpen(false);
      return;
    }

    const end = new Date(`${fullEnd}T00:00:00`);
    const start = new Date(end);
    let label = '';

    if (preset === 'thisMonth') {
      start.setDate(1);
      label = 'Este mês';
    } else if (preset === 'lastMonth') {
      start.setMonth(start.getMonth() - 1, 1);
      end.setDate(0);
      label = 'Mês passado';
    } else if (preset === 'last30') {
      start.setDate(start.getDate() - 29);
      label = 'Últimos 30 dias';
    } else if (preset === 'last3') {
      start.setMonth(start.getMonth() - 3);
      label = 'Últimos 3 meses';
    } else if (preset === 'last6') {
      start.setMonth(start.getMonth() - 6);
      label = 'Últimos 6 meses';
    } else {
      start.setMonth(start.getMonth() - 12);
      label = 'Últimos 12 meses';
    }

    const startValue = start.toISOString().slice(0, 10);
    const endValue = end.toISOString().slice(0, 10);
    applyRange(`${label} · ${formatDateInput(startValue)} - ${formatDateInput(endValue)}`, startValue, endValue);
  };

  const applyCustom = () => {
    if (!customStart || !customEnd) return;
    applyRange(`${formatDateInput(customStart)} - ${formatDateInput(customEnd)}`, customStart, customEnd);
  };

  return (
    <div className="relative rounded-xl border border-blue-100 bg-white/80 px-3 py-2">
      <button type="button" onClick={() => setOpen((current) => !current)} className="block w-full text-left">
        <div className="flex items-center justify-between gap-2">
          <p className="text-xs font-semibold text-gray-500">Período</p>
          <CalendarDays className="h-3.5 w-3.5 text-blue-700" />
        </div>
        <p className="mt-1 truncate text-sm font-extrabold text-gray-900">{value}</p>
      </button>

      {open ? (
        <div className="absolute right-0 top-[calc(100%+0.5rem)] z-30 w-[min(92vw,620px)] rounded-xl border border-gray-200 bg-white p-3 shadow-xl">
          <div className="grid gap-3 md:grid-cols-[180px_1fr]">
            <div className="space-y-1">
              {[
                ['thisMonth', 'Este mês'],
                ['lastMonth', 'Mês passado'],
                ['last30', 'Últimos 30 dias'],
                ['last3', 'Últimos 3 meses'],
                ['last6', 'Últimos 6 meses'],
                ['last12', 'Últimos 12 meses'],
                ['all', 'Todo o período'],
              ].map(([key, label]) => (
                <button
                  key={key}
                  type="button"
                  onClick={() => applyPreset(key as PresetKey)}
                  className="w-full rounded-lg px-3 py-2 text-left text-sm font-semibold text-gray-700 transition hover:bg-blue-50 hover:text-blue-700"
                >
                  {label}
                </button>
              ))}
            </div>

            <div className="rounded-lg border border-gray-100 bg-gray-50 p-3">
              <p className="text-xs font-bold uppercase tracking-wide text-gray-500">Personalizado</p>
              <div className="mt-3 grid gap-3 sm:grid-cols-2">
                <label className="text-xs font-semibold text-gray-600">
                  Início
                  <input
                    type="date"
                    value={customStart}
                    min={fullStart}
                    max={customEnd || fullEnd}
                    onChange={(event) => setCustomStart(event.target.value)}
                    className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm font-bold text-gray-900 outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-100"
                  />
                </label>
                <label className="text-xs font-semibold text-gray-600">
                  Fim
                  <input
                    type="date"
                    value={customEnd}
                    min={customStart || fullStart}
                    max={fullEnd}
                    onChange={(event) => setCustomEnd(event.target.value)}
                    className="mt-1 w-full rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm font-bold text-gray-900 outline-none focus:border-blue-400 focus:ring-2 focus:ring-blue-100"
                  />
                </label>
              </div>
              <div className="mt-4 flex justify-end gap-2">
                <button type="button" onClick={() => setOpen(false)} className="rounded-lg px-3 py-2 text-xs font-extrabold uppercase tracking-wide text-gray-500 hover:bg-gray-100">
                  Cancelar
                </button>
                <button type="button" onClick={applyCustom} className="rounded-lg bg-blue-600 px-3 py-2 text-xs font-extrabold uppercase tracking-wide text-white shadow-sm hover:bg-blue-700">
                  Aplicar
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
