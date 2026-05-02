import { Bar, BarChart, Cell, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { DashboardDistributionItem } from '../api';

const riskColors = ['#ef4444', '#f59e0b', '#5ba4e6', '#22c55e'];
const palette = ['#2272c3', '#5ba4e6', '#7c3aed', '#f59e0b', '#22c55e', '#ef4444', '#94a3b8'];

function formatPercent(value: number) {
  return `${value.toFixed(1)}%`;
}

export function MonthlyBars({ data, selectedLabel, onSelect }: { data: DashboardDistributionItem[]; selectedLabel?: string; onSelect?: (label: string) => void }) {
  return (
    <ResponsiveContainer width="100%" height={230}>
      <BarChart data={data}>
        <XAxis dataKey="label" tick={{ fontSize: 11 }} interval={0} angle={-20} textAnchor="end" height={54} />
        <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
        <Tooltip />
        <Bar
          dataKey="value"
          radius={[7, 7, 0, 0]}
          className="cursor-pointer"
          onClick={(entry) => {
            const payload = (entry as unknown as { payload?: DashboardDistributionItem }).payload;
            if (payload?.label) onSelect?.(payload.label);
          }}
        >
          {data.map((entry) => (
            <Cell
              key={entry.label}
              fill="#2272c3"
              opacity={!selectedLabel || selectedLabel === entry.label ? 1 : 0.35}
              stroke={selectedLabel === entry.label ? '#111827' : 'transparent'}
              strokeWidth={selectedLabel === entry.label ? 2 : 0}
            />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  );
}

export function DistributionDonut({
  data,
  selectedLabel,
  onSelect,
}: {
  data: DashboardDistributionItem[];
  selectedLabel?: string;
  onSelect?: (label: string) => void;
}) {
  const total = data.reduce((sum, item) => sum + item.value, 0);

  return (
    <div className="flex flex-col gap-4 md:flex-row md:items-center">
      <div className="h-56 w-full shrink-0 overflow-visible md:w-60">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={data}
              dataKey="value"
              nameKey="label"
              cx="50%"
              cy="50%"
              innerRadius={54}
              outerRadius={76}
              paddingAngle={2}
              className="cursor-pointer"
              onClick={(entry) => onSelect?.((entry as unknown as DashboardDistributionItem).label)}
            >
              {data.map((entry, index) => (
                <Cell
                  key={entry.label}
                  fill={riskColors[index % riskColors.length]}
                  opacity={!selectedLabel || selectedLabel === entry.label ? 1 : 0.35}
                  stroke={selectedLabel === entry.label ? '#111827' : '#ffffff'}
                  strokeWidth={selectedLabel === entry.label ? 3 : 1}
                />
              ))}
            </Pie>
            <Tooltip />
          </PieChart>
        </ResponsiveContainer>
      </div>
      <DistributionList data={data} colors={riskColors} total={total} selectedLabel={selectedLabel} onSelect={onSelect} />
    </div>
  );
}

export function HorizontalBars({ data, selectedLabel, onSelect }: { data: DashboardDistributionItem[]; selectedLabel?: string; onSelect?: (label: string) => void }) {
  const max = Math.max(...data.map((item) => item.value), 1);

  return (
    <div className="space-y-3">
      {data.map((item, index) => (
        <button
          key={item.label}
          type="button"
          onClick={() => onSelect?.(item.label)}
          className={`grid w-full grid-cols-[10rem_1fr_2.5rem] items-center gap-3 rounded-lg px-2 py-1.5 text-left transition hover:bg-gray-50 ${selectedLabel === item.label ? 'bg-blue-50 ring-1 ring-blue-100' : ''}`}
        >
          <span className="w-40 truncate text-xs font-semibold text-gray-600">{item.label}</span>
          <div className="h-2.5 flex-1 rounded-full bg-gray-100">
            <div
              className="h-full rounded-full transition-all"
              style={{
                width: `${(item.value * 100) / max}%`,
                backgroundColor: palette[index % palette.length],
                opacity: !selectedLabel || selectedLabel === item.label ? 1 : 0.35,
              }}
            />
          </div>
          <span className="w-10 text-right text-xs font-extrabold text-gray-800">{item.value}</span>
        </button>
      ))}
    </div>
  );
}

function DistributionList({
  data,
  colors,
  total,
  selectedLabel,
  onSelect,
}: {
  data: DashboardDistributionItem[];
  colors: string[];
  total: number;
  selectedLabel?: string;
  onSelect?: (label: string) => void;
}) {
  return (
    <div className="w-full space-y-2">
      {data.map((item, index) => (
        <button
          key={item.label}
          type="button"
          onClick={() => onSelect?.(item.label)}
          className={`flex w-full items-center justify-between gap-3 rounded-lg border-b border-gray-100 px-2 py-2 text-left transition last:border-b-0 hover:bg-gray-50 ${selectedLabel === item.label ? 'bg-blue-50 ring-1 ring-blue-100' : ''}`}
        >
          <div className="flex min-w-0 items-center gap-2">
            <span className="h-2.5 w-2.5 shrink-0 rounded-full" style={{ backgroundColor: colors[index % colors.length] }} />
            <span className="truncate text-sm font-semibold text-gray-700">
              {item.label} · {formatPercent((item.value * 100) / Math.max(total, 1))}
            </span>
          </div>
          <span className="text-sm font-extrabold text-gray-900">{item.value}</span>
        </button>
      ))}
    </div>
  );
}
