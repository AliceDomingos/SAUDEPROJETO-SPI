import type { Question } from '../types';

interface QuestionCardProps {
  question: Question;
  value: number | undefined;
  onChange: (score: number) => void;
}

export default function QuestionCard({ question, value, onChange }: QuestionCardProps) {
  return (
    <div className={`bg-white rounded-xl border-2 p-5 transition-all ${value ? 'border-blue-300 shadow-md' : 'border-gray-200'}`}>
      <div className="flex items-center gap-3 mb-4">
        <span className="w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-bold flex-shrink-0">
          {question.ordem ?? question.id}
        </span>
        <h3 className="text-base font-semibold">{question.name}</h3>
      </div>

      <div className="space-y-2 ml-11">
        {question.options.map((opt) => (
          <label
            key={opt.score}
            className={`flex items-center gap-3 p-3 rounded-lg border-2 cursor-pointer transition-all ${
              value === opt.score
                ? 'border-blue-500 bg-blue-50'
                : 'border-gray-200 hover:border-blue-300 hover:bg-blue-50/50'
            }`}
          >
            <input
              type="radio"
              name={`q-${question.id}`}
              className="accent-blue-600 w-4 h-4 flex-shrink-0"
              checked={value === opt.score}
              onChange={() => onChange(opt.score)}
            />
            <p className="text-sm">{opt.text}</p>
          </label>
        ))}
      </div>
    </div>
  );
}
