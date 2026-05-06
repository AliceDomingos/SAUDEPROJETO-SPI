export interface FormQuestionOption {
  valor: number;
  descricao: string;
}

export interface FormQuestion {
  id?: string;
  texto: string;
  peso: number;
  ordem: number;
  ativa?: boolean;
  opcoes: FormQuestionOption[];
}

export interface FaixaClassificacao {
  scoreMin: number;
  scoreMax: number;
  rotulo: string;
}

export interface Formulario {
  id: string;
  nome: string;
  descricao?: string;
  groupId?: string;
  groupNome?: string;
  criadoPorUsuarioId: string;
  criadoPorNome: string;
  ativo: boolean;
  pesoTotal: number;
  criadoEm: string;
  atualizadoEm: string;
  perguntas: FormQuestion[];
  faixas: FaixaClassificacao[];
}

export interface CriarFormularioPayload {
  nome: string;
  descricao?: string;
 groupId?: string;
  perguntas: { texto: string; peso: number; ordem: number; opcoes: FormQuestionOption[] }[];
  faixas: FaixaClassificacao[];
}

export interface Grupo {
  id: string;
  nome: string;
}
