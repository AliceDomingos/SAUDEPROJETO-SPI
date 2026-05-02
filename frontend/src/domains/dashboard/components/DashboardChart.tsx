import { Bar, BarChart, Cell, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { DashboardDistributionItem } from '../api';

interface DashboardChartProps {
  title: string;
  data: DashboardDistributionItem[];
  colors?: string[];
}

const defaultColors = ['#2563eb', '#ca8a04', '#dc2626', '#16a34a', '#7c3aed', '#0891b2'];

export default function DashboardChart({ title, data, colors = defaultColors }: DashboardChartProps) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-4">
      <h3 className="text-sm font-semibold text-gray-700 mb-4">{title}</h3>
      <ResponsiveContainer width="100%" height={240}>
        <BarChart data={data}>
          <XAxis dataKey="label" tick={{ fontSize: 11 }} interval={0} height={54} angle={-18} textAnchor="end" />
          <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
          <Tooltip />
          <Bar dataKey="value" radius={[6, 6, 0, 0]}>
            {data.map((entry, index) => (
              <Cell key={entry.label} fill={colors[index % colors.length]} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
