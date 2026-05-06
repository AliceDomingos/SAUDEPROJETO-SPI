import type { Specialist, SpecialistFormValues, SpecialistUpsertInput } from '../../types';

export function buildSpecialistFormValues(specialist?: Specialist | null): SpecialistFormValues {
  return {
    nome: specialist?.nome ?? '',
    especialidade: specialist?.especialidade ?? '',
    custoConsulta: specialist ? String(specialist.custoConsulta) : '',
    ativo: specialist?.ativo ?? true,
  };
}

export function mapSpecialistFormToInput(values: SpecialistFormValues): SpecialistUpsertInput {
  return {
    nome: values.nome.trim(),
    especialidade: values.especialidade.trim(),
    custoConsulta: parseMoney(values.custoConsulta),
    ativo: values.ativo,
  };
}

export function validateSpecialistForm(values: SpecialistFormValues) {
  if (!values.nome.trim()) {
    return 'Informe o nome do especialista.';
  }

  if (!values.especialidade.trim()) {
    return 'Informe a especialidade.';
  }

  if (parseMoney(values.custoConsulta) <= 0) {
    return 'Informe um valor de consulta maior que zero.';
  }

  return '';
}

export function parseMoney(value: string) {
  const normalized = value.replace(/\./g, '').replace(',', '.').replace(/[^\d.]/g, '');
  const parsed = Number(normalized);
  return Number.isFinite(parsed) ? parsed : 0;
}

export function formatCurrency(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value || 0);
}

export function formatSpecialistDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return '-';
  }

  return date.toLocaleDateString('pt-BR');
}

export function getSpecialistSearchText(specialist: Specialist) {
  return `${specialist.nome} ${specialist.especialidade}`.toLowerCase();
}
