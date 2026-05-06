import { useEffect, useState } from 'react';
import { getReusablePatients } from '@/domains/patients/api';
import type { Patient } from '@/types';

interface PatientSelectorProps {
  value: string | null;
  onChange: (id: string) => void;
  error?: string;
}

export default function PatientSelector({ value, onChange, error }: PatientSelectorProps) {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');

  useEffect(() => {
    getReusablePatients().then(setPatients);
  }, []);

  const filtered = patients.filter((p: { nome: string }) =>
    p.nome.toLowerCase().includes(search.toLowerCase())
  );

  const selected = patients.find((p) => p.id === value);

  return (
    <div className="space-y-1">
      <label className="block text-sm font-medium text-gray-700">Paciente</label>
      <div className="relative">
        <input
          value={selected?.nome || search}
          onChange={(e) => {
            setSearch(e.target.value);
            setOpen(true);
          }}
          onFocus={() => setOpen(true)}
          onBlur={() => setTimeout(() => setOpen(false), 200)}
          className={`w-full px-3 py-2 border rounded-lg outline-none transition ${
            error ? 'border-red-400' : 'border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500'
          }`}
          placeholder="Buscar paciente..."
        />
        {open && (
          <div className="absolute z-10 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg max-h-48 overflow-auto">
            {filtered.length === 0 && (
              <div className="p-3 text-sm text-gray-400 text-center">Nenhum paciente encontrado</div>
            )}
            {filtered.map((p) => (
              <button
                key={p.id}
                type="button"
                className={`w-full text-left px-3 py-2 text-sm hover:bg-blue-50 transition ${
                  p.id === value ? 'bg-blue-50 text-blue-700 font-medium' : ''
                }`}
                onMouseDown={() => {
                  onChange(p.id);
                  setOpen(false);
                  setSearch('');
                }}
              >
                {p.nome} — {p.idade || '?'} anos
              </button>
            ))}
          </div>
        )}
      </div>
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  );
}
