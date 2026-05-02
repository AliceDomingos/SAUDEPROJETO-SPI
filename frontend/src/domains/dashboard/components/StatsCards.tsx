interface StatItem {
  label: string;
  value: string | number;
  icon: React.ComponentType<{ className?: string }>;
  tone?: 'blue' | 'green' | 'amber' | 'red' | 'purple' | 'slate';
  tag?: string;
  foot?: string;
  secondaryLabel?: string;
  secondaryValue?: string | number;
  progressValue?: number;
}

interface StatsCardsProps {
  items: StatItem[];
  columnsClassName?: string;
}

const tones = {
  blue: 'bg-blue-50 text-blue-700 border-blue-100',
  green: 'bg-emerald-50 text-emerald-700 border-emerald-100',
  amber: 'bg-amber-50 text-amber-700 border-amber-100',
  red: 'bg-red-50 text-red-700 border-red-100',
  purple: 'bg-violet-50 text-violet-700 border-violet-100',
  slate: 'bg-slate-50 text-slate-700 border-slate-100',
};

const toneBars = {
  blue: 'after:bg-blue-500',
  green: 'after:bg-emerald-500',
  amber: 'after:bg-amber-500',
  red: 'after:bg-red-500',
  purple: 'after:bg-violet-500',
  slate: 'after:bg-slate-500',
};

const progressTones = {
  blue: 'bg-blue-500',
  green: 'bg-emerald-500',
  amber: 'bg-amber-500',
  red: 'bg-red-500',
  purple: 'bg-violet-500',
  slate: 'bg-slate-500',
};

export default function StatsCards({ items, columnsClassName = 'grid-cols-1 sm:grid-cols-2 xl:grid-cols-4' }: StatsCardsProps) {
  return (
    <div className={`grid ${columnsClassName} gap-4`}>
      {items.map((item) => (
        <CardValue key={item.label} {...item} />
      ))}
    </div>
  );
}

function CardValue({ icon: Icon, label, value, tone = 'blue', tag, foot, secondaryLabel, secondaryValue, progressValue }: StatItem) {
  const hasSecondaryMetric = secondaryLabel && secondaryValue !== undefined;
  const progress = progressValue === undefined ? null : Math.min(Math.max(progressValue, 0), 100);

  return (
    <div className={`relative overflow-hidden bg-white rounded-xl border border-gray-200 p-4 min-h-[126px] shadow-sm after:absolute after:inset-x-0 after:bottom-0 after:h-1 after:content-[''] ${toneBars[tone]}`}>
      <div className="flex items-start justify-between gap-3">
        <div className={`w-11 h-11 rounded-xl flex items-center justify-center border ${tones[tone]}`}>
          <Icon className="w-5 h-5" />
        </div>
        {tag ? <span className="rounded-md bg-gray-100 px-2 py-1 text-[10px] font-bold uppercase text-gray-500">{tag}</span> : null}
      </div>
      <p className="mt-4 text-xs font-bold uppercase tracking-wide text-gray-500">{label}</p>
      {hasSecondaryMetric ? (
        <>
          <div className="mt-3 flex flex-col gap-4 sm:flex-row sm:items-end">
            <div className="min-w-0 flex-1">
              <p className="text-[11px] font-bold uppercase tracking-wide text-gray-500">Total</p>
              <p className="mt-1 text-4xl font-extrabold leading-none tracking-tight text-gray-900">{String(value)}</p>
            </div>
            <div className="hidden h-12 w-px shrink-0 bg-gray-200 sm:block" />
            <div className="min-w-0 flex-1">
              <p className="text-[11px] font-bold uppercase tracking-wide text-gray-500">{secondaryLabel}</p>
              <p className="mt-1 text-4xl font-extrabold leading-none tracking-tight text-gray-900">{String(secondaryValue)}</p>
            </div>
          </div>
          {progress !== null ? (
            <div className="mt-4">
              <div className="h-2 overflow-hidden rounded-full bg-gray-100">
                <div className={`h-full rounded-full ${progressTones[tone]}`} style={{ width: `${progress}%` }} />
              </div>
            </div>
          ) : null}
        </>
      ) : (
        <p className="mt-1 text-3xl font-extrabold tracking-tight text-gray-900">{String(value)}</p>
      )}
      {foot ? <p className="mt-2 text-xs font-medium text-gray-500">{foot}</p> : null}
    </div>
  );
}
