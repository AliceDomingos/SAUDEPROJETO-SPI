import type { SpecialistFormValues } from '../../types';

interface SpecialistFormFieldsProps {
  values: SpecialistFormValues;
  onChange: <K extends keyof SpecialistFormValues>(field: K, value: SpecialistFormValues[K]) => void;
  disabled?: boolean;
  showStatus?: boolean;
}

export default function SpecialistFormFields({
  values,
  onChange,
  disabled = false,
  showStatus = false,
}: SpecialistFormFieldsProps) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <div className="md:col-span-2">
        <label className="mb-1 block text-sm font-medium">Nome do especialista</label>
        <input
          value={values.nome}
          onChange={(event) => onChange('nome', event.target.value)}
          required
          disabled={disabled}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          placeholder="Ex.: Dra. Maria Silva"
        />
      </div>

      <div>
        <label className="mb-1 block text-sm font-medium">Especialidade</label>
        <input
          value={values.especialidade}
          onChange={(event) => onChange('especialidade', event.target.value)}
          required
          disabled={disabled}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          placeholder="Ex.: Fonoaudiologo"
        />
      </div>

      <div>
        <label className="mb-1 block text-sm font-medium">Valor da consulta</label>
        <input
          value={values.custoConsulta}
          onChange={(event) => onChange('custoConsulta', event.target.value)}
          required
          inputMode="decimal"
          disabled={disabled}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          placeholder="Ex.: 350,00"
        />
      </div>

      {showStatus && (
        <label className="inline-flex items-center gap-2 text-sm font-medium text-gray-700">
          <input
            type="checkbox"
            checked={values.ativo}
            onChange={(event) => onChange('ativo', event.target.checked)}
            disabled={disabled}
            className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          Especialista ativo
        </label>
      )}
    </div>
  );
}
