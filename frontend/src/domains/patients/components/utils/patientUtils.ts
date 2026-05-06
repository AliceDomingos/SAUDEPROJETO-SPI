import type { Patient } from '@/types';
import type {
  PatientFormValues,
  PatientSearchField,
  PatientSex,
  PatientUpsertInput,
} from '../../types';

export const patientSexOptions: { value: PatientSex; label: string }[] = [
  { value: 'feminino', label: 'Feminino' },
  { value: 'masculino', label: 'Masculino' },
  { value: 'outro', label: 'Outro' },
];

export const brazilianStateOptions = [
  { value: 'AC', label: 'AC - Acre' },
  { value: 'AL', label: 'AL - Alagoas' },
  { value: 'AP', label: 'AP - Amapa' },
  { value: 'AM', label: 'AM - Amazonas' },
  { value: 'BA', label: 'BA - Bahia' },
  { value: 'CE', label: 'CE - Ceara' },
  { value: 'DF', label: 'DF - Distrito Federal' },
  { value: 'ES', label: 'ES - Espirito Santo' },
  { value: 'GO', label: 'GO - Goias' },
  { value: 'MA', label: 'MA - Maranhao' },
  { value: 'MT', label: 'MT - Mato Grosso' },
  { value: 'MS', label: 'MS - Mato Grosso do Sul' },
  { value: 'MG', label: 'MG - Minas Gerais' },
  { value: 'PA', label: 'PA - Para' },
  { value: 'PB', label: 'PB - Paraiba' },
  { value: 'PR', label: 'PR - Parana' },
  { value: 'PE', label: 'PE - Pernambuco' },
  { value: 'PI', label: 'PI - Piaui' },
  { value: 'RJ', label: 'RJ - Rio de Janeiro' },
  { value: 'RN', label: 'RN - Rio Grande do Norte' },
  { value: 'RS', label: 'RS - Rio Grande do Sul' },
  { value: 'RO', label: 'RO - Rondonia' },
  { value: 'RR', label: 'RR - Roraima' },
  { value: 'SC', label: 'SC - Santa Catarina' },
  { value: 'SP', label: 'SP - Sao Paulo' },
  { value: 'SE', label: 'SE - Sergipe' },
  { value: 'TO', label: 'TO - Tocantins' },
] as const;

export const patientSearchFieldOptions: { value: PatientSearchField; label: string }[] = [
  { value: 'all', label: 'Todas as colunas' },
  { value: 'nome', label: 'Nome' },
  { value: 'cpf', label: 'CPF' },
  { value: 'sexo', label: 'Sexo' },
  { value: 'data_nascimento', label: 'Data de nascimento' },
];

export function buildPatientFormValues(patient?: Patient | null): PatientFormValues {
  const sexo = patient?.sexo;

  return {
    nome: patient?.nome ?? '',
    cpf: maskCpfInput(patient?.cpf ?? ''),
    dataNascimento: normalizeDateInputValue(patient?.data_nascimento),
    sexo: sexo === 'feminino' || sexo === 'masculino' || sexo === 'outro' ? sexo : '',
    groupId: patient?.group_id ? String(patient.group_id) : '',
    nomeResponsavel: patient?.nome_responsavel ?? '',
    telefone: maskPhoneInput(patient?.telefone ?? ''),
    email: patient?.email ?? '',
    cep: maskCepInput(patient?.cep ?? ''),
    estado: normalizeBrazilianState(patient?.estado),
    cidade: patient?.cidade ?? '',
    bairro: patient?.bairro ?? '',
    rua: patient?.rua ?? '',
    numero: patient?.numero ?? '',
    complemento: patient?.complemento ?? '',
    observacoes: patient?.observacoes ?? '',
  };
}

export function mapPatientFormToInput(values: PatientFormValues): PatientUpsertInput {
  if (!values.sexo) {
    throw new Error('Informe o sexo do paciente.');
  }

  const telefone = unmaskDigits(values.telefone);
  const email = values.email.trim();
  const nomeResponsavel = values.nomeResponsavel.trim();
  const cep = unmaskDigits(values.cep);
  const estado = normalizeBrazilianState(values.estado);
  const cidade = values.cidade.trim();
  const bairro = values.bairro.trim();
  const rua = values.rua.trim();
  const numero = values.numero.trim();
  const complemento = values.complemento.trim();
  const observacoes = values.observacoes.trim();

  return {
    nome: values.nome.trim(),
    cpf: unmaskDigits(values.cpf),
    data_nascimento: values.dataNascimento,
    sexo: values.sexo,
    groupId: values.groupId || undefined,
    nome_responsavel: nomeResponsavel || undefined,
    telefone: telefone || undefined,
    email: email || undefined,
    cep: cep || undefined,
    estado: estado || undefined,
    cidade: cidade || undefined,
    bairro: bairro || undefined,
    rua: rua || undefined,
    numero: numero || undefined,
    complemento: complemento || undefined,
    observacoes: observacoes || undefined,
  };
}

export function validatePatientForm(
  values: PatientFormValues,
  options?: { requireGroup?: boolean },
): string | null {
  if (!values.nome.trim()) {
    return 'Informe o nome completo do paciente.';
  }

  if (unmaskDigits(values.cpf).length !== 11) {
    return 'Informe um CPF valido com 11 digitos.';
  }

  if (!values.dataNascimento) {
    return 'Informe a data de nascimento.';
  }

  const birthDate = new Date(values.dataNascimento);
  if (Number.isNaN(birthDate.getTime())) {
    return 'Informe uma data de nascimento valida.';
  }

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  if (birthDate > today) {
    return 'A data de nascimento nao pode estar no futuro.';
  }

  if (!values.sexo) {
    return 'Informe o sexo do paciente.';
  }

  if (options?.requireGroup && !values.groupId) {
    return 'Selecione o grupo do paciente.';
  }

  if (values.email.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(values.email.trim())) {
    return 'Informe um e-mail valido.';
  }

  const cep = unmaskDigits(values.cep);
  if (cep && cep.length !== 8) {
    return 'Informe um CEP valido com 8 digitos.';
  }

  if (values.estado && !isValidBrazilianState(values.estado)) {
    return 'Selecione uma UF valida.';
  }

  return null;
}

export function formatPatientSex(value?: string | null): string {
  switch (value) {
    case 'feminino':
      return 'Feminino';
    case 'masculino':
      return 'Masculino';
    case 'outro':
      return 'Outro';
    case 'nao_informado':
    default:
      return 'Nao informado';
  }
}

export function formatDate(value?: string | null): string {
  if (!value) {
    return 'Nao informado';
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return parsed.toLocaleDateString('pt-BR');
}

export function formatCpf(value?: string | null): string {
  const digits = unmaskDigits(value);
  if (digits.length !== 11) {
    return value?.trim() || 'Nao informado';
  }

  return digits.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
}

export function formatPhone(value?: string | null): string {
  const digits = unmaskDigits(value);
  if (!digits) {
    return 'Nao informado';
  }

  if (digits.length === 11) {
    return digits.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
  }

  if (digits.length === 10) {
    return digits.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
  }

  return value?.trim() || 'Nao informado';
}

export function formatCep(value?: string | null): string {
  const digits = unmaskDigits(value);
  if (!digits) {
    return 'Nao informado';
  }

  if (digits.length === 8) {
    return digits.replace(/(\d{5})(\d{3})/, '$1-$2');
  }

  return value?.trim() || 'Nao informado';
}

export function formatBrazilianState(value?: string | null): string {
  const normalized = normalizeBrazilianState(value);
  if (!normalized) {
    return 'Nao informado';
  }

  const option = brazilianStateOptions.find((item) => item.value === normalized);
  return option?.label ?? normalized;
}

export function formatPatientAddress(patient?: Patient | null): string {
  if (!patient) {
    return 'Nao informado';
  }

  const line = [patient.rua, patient.numero, patient.complemento].filter(Boolean).join(', ');
  const district = [patient.bairro, patient.cidade, patient.estado].filter(Boolean).join(' - ');
  const cep = patient.cep ? `CEP ${formatCep(patient.cep)}` : '';

  return [line, district, cep].filter(Boolean).join(' | ') || 'Nao informado';
}

export function maskCpfInput(value: string): string {
  const digits = unmaskDigits(value).slice(0, 11);

  if (digits.length <= 3) {
    return digits;
  }

  if (digits.length <= 6) {
    return digits.replace(/(\d{3})(\d+)/, '$1.$2');
  }

  if (digits.length <= 9) {
    return digits.replace(/(\d{3})(\d{3})(\d+)/, '$1.$2.$3');
  }

  return digits.replace(/(\d{3})(\d{3})(\d{3})(\d+)/, '$1.$2.$3-$4');
}

export function maskCepInput(value: string): string {
  const digits = unmaskDigits(value).slice(0, 8);

  if (digits.length <= 5) {
    return digits;
  }

  return digits.replace(/(\d{5})(\d+)/, '$1-$2');
}

export function maskPhoneInput(value: string): string {
  const digits = unmaskDigits(value).slice(0, 11);

  if (digits.length <= 2) {
    return digits;
  }

  if (digits.length <= 6) {
    return digits.replace(/(\d{2})(\d+)/, '($1) $2');
  }

  if (digits.length <= 10) {
    return digits.replace(/(\d{2})(\d{4})(\d+)/, '($1) $2-$3');
  }

  return digits.replace(/(\d{2})(\d{5})(\d+)/, '($1) $2-$3');
}

export function getPatientSearchText(patient: Patient): string {
  return [
    patient.nome,
    patient.cpf,
    formatCpf(patient.cpf),
    patient.email,
    patient.nome_responsavel,
    patient.telefone,
    formatPhone(patient.telefone),
    formatPatientSex(patient.sexo),
    patient.data_nascimento,
    formatDate(patient.data_nascimento),
    patient.group_nome,
    patient.cep,
    formatCep(patient.cep),
    patient.estado,
    patient.cidade,
    patient.bairro,
    patient.rua,
    patient.numero,
    patient.complemento,
    formatPatientAddress(patient),
  ]
    .filter(Boolean)
    .join(' ')
    .toLowerCase();
}

export function matchesPatientSearch(
  patient: Patient,
  rawSearch: string,
  field: PatientSearchField,
): boolean {
  const normalizedSearch = rawSearch.trim().toLowerCase();
  if (!normalizedSearch) {
    return true;
  }

  if (field === 'all') {
    return getPatientSearchText(patient).includes(normalizedSearch);
  }

  return getPatientSearchValue(patient, field).includes(normalizedSearch);
}

export function getPatientAgeLabel(patient: Patient): string {
  if (typeof patient.idade === 'number') {
    return `${patient.idade} ano(s)`;
  }

  if (!patient.data_nascimento) {
    return 'Nao informado';
  }

  const birthDate = new Date(patient.data_nascimento);
  if (Number.isNaN(birthDate.getTime())) {
    return 'Nao informado';
  }

  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  const monthDiff = today.getMonth() - birthDate.getMonth();
  const dayDiff = today.getDate() - birthDate.getDate();

  if (monthDiff < 0 || (monthDiff === 0 && dayDiff < 0)) {
    age -= 1;
  }

  return age >= 0 ? `${age} ano(s)` : 'Nao informado';
}

function unmaskDigits(value?: string | null): string {
  return (value ?? '').replace(/\D/g, '');
}

function normalizeBrazilianState(value?: string | null): string {
  const normalized = (value ?? '').trim().toUpperCase();
  return isValidBrazilianState(normalized) ? normalized : normalized;
}

function isValidBrazilianState(value?: string | null): boolean {
  if (!value) {
    return false;
  }

  return brazilianStateOptions.some((item) => item.value === value.trim().toUpperCase());
}

function normalizeDateInputValue(value?: string | null): string {
  if (!value) {
    return '';
  }

  const trimmed = value.trim();
  const isoDateMatch = trimmed.match(/^(\d{4}-\d{2}-\d{2})/);
  if (isoDateMatch) {
    return isoDateMatch[1];
  }

  const parsed = new Date(trimmed);
  if (Number.isNaN(parsed.getTime())) {
    return '';
  }

  const year = parsed.getFullYear();
  const month = String(parsed.getMonth() + 1).padStart(2, '0');
  const day = String(parsed.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function getPatientSearchValue(patient: Patient, field: PatientSearchField): string {
  switch (field) {
    case 'nome':
      return patient.nome.toLowerCase();
    case 'cpf':
      return `${patient.cpf ?? ''} ${formatCpf(patient.cpf)}`.toLowerCase();
    case 'sexo':
      return formatPatientSex(patient.sexo).toLowerCase();
    case 'data_nascimento':
      return `${patient.data_nascimento ?? ''} ${formatDate(patient.data_nascimento)}`.toLowerCase();
    case 'all':
    default:
      return getPatientSearchText(patient);
  }
}
