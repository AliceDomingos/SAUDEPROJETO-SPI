import type { User } from '@/types';
import type { GroupFormValues } from '../../types';

interface GroupFormFieldsProps {
  values: GroupFormValues;
  onChange: (field: keyof GroupFormValues, value: string) => void;
  managers: User[];
  disabled?: boolean;
  showManagerField?: boolean;
  managerHint?: string;
}

export default function GroupFormFields({
  values,
  onChange,
  managers,
  disabled = false,
  showManagerField = true,
  managerHint,
}: GroupFormFieldsProps) {
  return (
    <div className="grid gap-4 md:grid-cols-2">
      <div className="md:col-span-2">
        <label className="mb-1 block text-sm font-medium">Nome do grupo</label>
        <input
          value={values.nome}
          onChange={(event) => onChange('nome', event.target.value)}
          required
          disabled={disabled}
          className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          placeholder="Nome do grupo"
        />
      </div>

      {showManagerField && (
        <div className="md:col-span-2">
          <label className="mb-1 block text-sm font-medium">Gestor responsavel <span className="text-gray-400 font-normal">(opcional)</span></label>
          <select
            value={values.gestorId}
            onChange={(event) => onChange('gestorId', event.target.value)}
            disabled={disabled}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 outline-none focus:ring-2 focus:ring-blue-500 disabled:cursor-not-allowed disabled:bg-gray-100"
          >
            <option value="">Sem gestor por enquanto</option>
            {managers.map((manager) => (
              <option key={manager.id} value={String(manager.id)}>
                {manager.nome}
              </option>
            ))}
          </select>
          {managerHint && <p className="mt-1 text-xs text-gray-500">{managerHint}</p>}
        </div>
      )}
    </div>
  );
}
