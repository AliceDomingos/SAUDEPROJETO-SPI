import SearchFiltersPanel from '@/shared/components/filters/SearchFiltersPanel';
import type { PatientSearchField } from '../../types';
import { patientSearchFieldOptions } from '../utils/patientUtils';

interface PatientFiltersBarProps {
  search: string;
  searchField: PatientSearchField;
  onSearchChange: (value: string) => void;
  onSearchFieldChange: (value: PatientSearchField) => void;
  onClear: () => void;
}

export default function PatientFiltersBar({
  search,
  searchField,
  onSearchChange,
  onSearchFieldChange,
  onClear,
}: PatientFiltersBarProps) {
  const searchPlaceholderByField: Record<PatientSearchField, string> = {
    all: 'Buscar por nome, CPF, sexo ou data de nascimento',
    nome: 'Buscar paciente pelo nome',
    cpf: 'Buscar paciente pelo CPF',
    sexo: 'Buscar paciente pelo sexo',
    data_nascimento: 'Buscar paciente pela data de nascimento',
  };

  const hasActiveFilters = search.trim().length > 0 || searchField !== 'all';

  return (
    <SearchFiltersPanel
      title="Encontre pacientes com mais rapidez"
      description="Use a busca principal para localizar por nome, CPF ou outros dados e refine se precisar."
      searchLabel="Buscar paciente"
      searchValue={search}
      searchPlaceholder={searchPlaceholderByField[searchField]}
      onSearchChange={onSearchChange}
      hasActiveFilters={search.trim().length > 0}
      onClear={onClear}
    />
  );
}
