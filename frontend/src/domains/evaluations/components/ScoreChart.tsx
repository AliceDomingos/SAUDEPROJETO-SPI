import {
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  Radar,
  ResponsiveContainer,
} from 'recharts';
import { SPI_QUESTIONS } from '../utils/questions';

interface ScoreChartProps {
  respostas: Record<string, number>;
  questions?: { id: number; name: string }[];
}

export default function ScoreChart({ respostas, questions }: ScoreChartProps) {
  const qs = questions ?? SPI_QUESTIONS;
  const data = qs.map((q) => ({
    dimensao: q.name,
    score: respostas[q.id] || 0,
  }));

  return (
    <div className="bg-white rounded-xl border border-gray-200 p-4">
      <h3 className="text-sm font-semibold text-gray-700 mb-2 text-center">Perfil por Dimensão</h3>
      <ResponsiveContainer width="100%" height={260}>
        <RadarChart data={data}>
          <PolarGrid />
          <PolarAngleAxis dataKey="dimensao" tick={{ fontSize: 9 }} />
          <Radar name="Score" dataKey="score" stroke="#2563eb" fill="#2563eb" fillOpacity={0.3} />
        </RadarChart>
      </ResponsiveContainer>
    </div>
  );
}



