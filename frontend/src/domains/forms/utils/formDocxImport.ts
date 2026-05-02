import JSZip from 'jszip';
import type { FormImportData } from './formExport';

const OPTION_REGEX = /^(\d+)\s*[-–]\s*(.+)$/;
const STOP_MARKERS = ['impressões gerais', 'pontuaç'];
const HEADER_SKIP = /^(nome\s+da|data\s+da)/i;

async function extractParagraphs(file: File): Promise<string[]> {
  const buffer = await file.arrayBuffer();
  const zip = await JSZip.loadAsync(buffer);
  const entry = zip.file('word/document.xml');
  if (!entry) throw new Error('Arquivo .docx inválido: estrutura não reconhecida.');
  const xmlRaw = await entry.async('string');

  const doc = new DOMParser().parseFromString(xmlRaw, 'application/xml');
  const ns = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main';
  const paragraphs = Array.from(doc.getElementsByTagNameNS(ns, 'p'));

  return paragraphs
    .map((p) => {
      const texts = Array.from(p.getElementsByTagNameNS(ns, 't'));
      return texts.map((t) => t.textContent ?? '').join('').trim();
    })
    .filter(Boolean);
}

export async function parseDocxForm(file: File): Promise<FormImportData> {
  const lines = await extractParagraphs(file);

  // Localiza o índice da primeira opção (linha "1 - ...") para saber onde as perguntas começam
  const firstOptionIndex = lines.findIndex((l) => OPTION_REGEX.test(l));
  if (firstOptionIndex < 1) {
    throw new Error('Formato não reconhecido: nenhuma opção de resposta encontrada no documento.');
  }

  // A primeira pergunta é o parágrafo imediatamente antes da primeira opção
  const firstQuestionIndex = firstOptionIndex - 1;

  // Extrai o nome do formulário do cabeçalho: primeira linha curta que não seja campo de preenchimento
  const headerLines = lines.slice(0, firstQuestionIndex);
  const nome =
    headerLines.find((l) => l.length < 120 && !HEADER_SKIP.test(l)) ?? 'Formulário importado';

  // Parseia perguntas e opções a partir da primeira pergunta
  const perguntas: FormImportData['perguntas'] = [];
  let current: FormImportData['perguntas'][0] | null = null;
  let ordem = 1;

  for (const line of lines.slice(firstQuestionIndex)) {
    if (STOP_MARKERS.some((m) => line.toLowerCase().includes(m))) break;

    const match = line.match(OPTION_REGEX);
    if (match) {
      if (current) {
        (current.opcoes ??= []).push({
          valor: parseInt(match[1], 10),
          descricao: match[2].trim(),
        });
      }
    } else {
      if (current) perguntas.push(current);
      current = { texto: line, peso: 4, ordem: ordem++, opcoes: [] };
    }
  }
  if (current && current.opcoes && current.opcoes.length > 0) perguntas.push(current);

  if (perguntas.length === 0) {
    throw new Error('Nenhuma pergunta encontrada no documento.');
  }

  // Peso = valor máximo entre as opções
  for (const p of perguntas) {
    if (p.opcoes && p.opcoes.length > 0) {
      p.peso = Math.max(...p.opcoes.map((o) => o.valor));
    }
  }

  return { nome, perguntas };
}
