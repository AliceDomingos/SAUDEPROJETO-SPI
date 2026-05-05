import type { Question } from '../types';

interface QuestionCardProps {
  question: Question;
  value: number | undefined;
  onChange: (score: number) => void;
}

export default function QuestionCard({ question, value, onChange }: QuestionCardProps) {
  const labels = ['Normal', 'Leve', 'Moderado', 'Grave'];

  return (
    <div className={`bg-white rounded-xl border-2 p-5 transition-all ${value ? 'border-blue-300 shadow-md' : 'border-gray-200'}`}>
      <div className="flex items-center gap-3 mb-4">
        <span className="w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-bold flex-shrink-0">
          {question.ordem ?? question.id}
        </span>
        <h3 className="text-base font-semibold">{question.name}</h3>
      </div>

      <div className="space-y-2 ml-11">
        {question.options.map((opt, i) => (
          <label
            key={opt.score}
            className={`flex items-start gap-3 p-3 rounded-lg border-2 cursor-pointer transition-all ${
              value === opt.score
                ? 'border-blue-500 bg-blue-50'
                : 'border-gray-200 hover:border-blue-300 hover:bg-blue-50/50'
            }`}
          >
            <input
              type="radio"
              name={`q-${question.id}`}
              className="mt-1 accent-blue-600 w-4 h-4"
              checked={value === opt.score}
              onChange={() => onChange(opt.score)}
            />
            <div className="flex-1">
              <p className="text-sm">{opt.text}</p>
            </div>
            <span
              className={`text-xs font-bold px-2 py-1 rounded-full flex-shrink-0 ${
                value === opt.score
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-500'
              }`}
            >
              {opt.score} — {labels[i]}
            </span>
          </label>
        ))}
      </div>
    </div>
  );
}
