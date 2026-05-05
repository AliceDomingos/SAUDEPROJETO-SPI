import { jsPDF } from 'jspdf';
import type { Evaluation } from '@/types';
import type { Formulario } from '@/domains/forms/types';
import { SPI_QUESTIONS } from './questions';

interface NormalizedQuestion {
  id: number;
  texto: string;
  opcoes: { valor: number; descricao: string }[];
}

function getQuestions(form?: Formulario): NormalizedQuestion[] {
  if (form) {
    return form.perguntas
      .sort((a, b) => a.ordem - b.ordem)
      .map((p) => ({ id: p.id!, texto: p.texto, opcoes: p.opcoes }));
  }
  return SPI_QUESTIONS.map((q) => ({
    id: q.id,
    texto: q.name,
    opcoes: q.options.map((o) => ({ valor: o.score, descricao: o.text })),
  }));
}

function buildEvalPdfDoc(evaluation: Evaluation, form?: Formulario): jsPDF {
  const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });

  const mL = 14;
  const pageW = doc.internal.pageSize.getWidth();
  const pageH = doc.internal.pageSize.getHeight();
  const cW = pageW - mL * 2;
  let y = 14;

  const questions = getQuestions(form);
  const formNome = form?.nome ?? 'SPI — Avaliação de Comportamento';

  // ── Cabeçalho ─────────────────────────────────────────────────────────
  doc.setFontSize(7);
  doc.setFont('helvetica', 'bold');
  doc.setTextColor(140, 140, 140);
  doc.text('AVALIAÇÃO REALIZADA', mL, y);

  y += 5;
  doc.setFontSize(15);
  doc.setFont('helvetica', 'bold');
  doc.setTextColor(20, 20, 20);
  const titleLines = doc.splitTextToSize(formNome, cW) as string[];
  doc.text(titleLines, mL, y);
  y += titleLines.length * 7 + 1;

  // Barra de metadados
  const scoreColor: [number, number, number] =
    evaluation.scoreTotal <= 29.5 ? [22, 163, 74] : evaluation.scoreTotal < 37 ? [202, 138, 4] : [220, 38, 38];

  doc.setFillColor(243, 244, 246);
  doc.setDrawColor(209, 213, 219);
  doc.setLineWidth(0.3);
  doc.rect(mL, y, cW, 10, 'FD');
  doc.setFontSize(7.5);
  doc.setFont('helvetica', 'normal');
  doc.setTextColor(80, 80, 80);
  const dataStr = new Date(evaluation.dataAvaliacao).toLocaleDateString('pt-BR');
  const metaLeft = `Paciente: ${evaluation.patientNome}     Avaliador: ${evaluation.avaliadorNome}     Data: ${dataStr}`;
  doc.text(metaLeft, mL + 3, y + 4.2);

  doc.setFont('helvetica', 'bold');
  doc.setTextColor(...scoreColor);
  const scoreStr = `Score: ${evaluation.scoreTotal}/${evaluation.pesoTotal}   ${evaluation.classificacao}`;
  doc.text(scoreStr, mL + cW - 3, y + 4.2, { align: 'right' });
  y += 15;

  // ── Perguntas ──────────────────────────────────────────────────────────
  const numW = 14;
  const textX = mL + numW;

  questions.forEach((q, i) => {
    const respostaMarcada = evaluation.respostas[q.id];

    const headerTextW = cW - numW - 5;
    const tituloLinhas = doc.splitTextToSize(q.texto, headerTextW) as string[];
    const headerH = Math.max(10, tituloLinhas.length * 5.5 + 4);

    const circleX = textX + 5.5;
    const valX = textX + 12;
    const optTxtX = textX + 18;
    const optTxtW = cW - numW - 20;

    const opcaoLinhasArr = q.opcoes.map(
      (o) => doc.splitTextToSize(o.descricao, optTxtW) as string[]
    );
    const optionsH = opcaoLinhasArr.reduce(
      (s, l) => s + Math.max(7, l.length * 4.5 + 2.5),
      0
    );
    const totalH = headerH + optionsH;

    if (y + totalH > pageH - 16) { doc.addPage(); y = 14; }

    const startY = y;

    // Borda externa
    doc.setDrawColor(209, 213, 219);
    doc.setLineWidth(0.3);
    doc.rect(mL, startY, cW, totalH, 'D');

    // Fundo cinza cabeçalho
    doc.setFillColor(243, 244, 246);
    doc.rect(mL, startY, cW, headerH, 'F');

    // Divisória vertical cabeçalho
    doc.setDrawColor(209, 213, 219);
    doc.setLineWidth(0.3);
    doc.line(textX, startY, textX, startY + headerH);

    // Número
    doc.setFontSize(10);
    doc.setFont('helvetica', 'bold');
    doc.setTextColor(100, 100, 100);
    doc.text(
      String(i + 1).padStart(2, '0'),
      mL + numW / 2,
      startY + headerH / 2 + 1.8,
      { align: 'center' }
    );

    // Título da pergunta
    doc.setFontSize(9.5);
    doc.setFont('helvetica', 'bold');
    doc.setTextColor(20, 20, 20);
    doc.text(tituloLinhas, textX + 4, startY + 6);

    // Separador cabeçalho / opções
    doc.setDrawColor(209, 213, 219);
    doc.setLineWidth(0.3);
    doc.line(mL, startY + headerH, mL + cW, startY + headerH);

    // Opções
    let oy = startY + headerH;
    q.opcoes.forEach((o, oi) => {
      const linhas = opcaoLinhasArr[oi];
      const optH = Math.max(7, linhas.length * 4.5 + 2.5);
      const isSelected = o.valor === respostaMarcada;

      // Fundo azul claro na opção selecionada
      if (isSelected) {
        doc.setFillColor(239, 246, 255);
        doc.rect(textX, oy, cW - numW, optH, 'F');
      }

      // Separador entre opções
      if (oi > 0) {
        doc.setDrawColor(229, 231, 235);
        doc.setLineWidth(0.2);
        doc.line(textX, oy, mL + cW, oy);
      }

      // Divisória vertical
      doc.setDrawColor(209, 213, 219);
      doc.setLineWidth(0.3);
      doc.line(textX, oy, textX, oy + optH);

      // Círculo — preenchido se selecionado, vazio caso contrário
      if (isSelected) {
        doc.setFillColor(37, 99, 235);
        doc.setDrawColor(37, 99, 235);
        doc.setLineWidth(0.4);
        doc.circle(circleX, oy + optH / 2, 2.2, 'FD');
      } else {
        doc.setDrawColor(160, 160, 160);
        doc.setLineWidth(0.4);
        doc.circle(circleX, oy + optH / 2, 2.2, 'D');
      }

      // Valor da opção
      doc.setFontSize(8);
      doc.setFont('helvetica', isSelected ? 'bold' : 'bold');
      doc.setTextColor(isSelected ? 37 : 60, isSelected ? 99 : 60, isSelected ? 235 : 60);
      doc.text(String(o.valor), valX, oy + optH / 2 + 1.2, { align: 'center' });

      // Descrição da opção
      doc.setFontSize(8.5);
      doc.setFont('helvetica', isSelected ? 'bold' : 'normal');
      doc.setTextColor(isSelected ? 30 : 30, isSelected ? 58 : 30, isSelected ? 138 : 30);
      doc.text(linhas, optTxtX, oy + (optH - linhas.length * 4.5) / 2 + 4);

      oy += optH;
    });

    y = startY + totalH + 3;
  });

  // ── Rodapé ─────────────────────────────────────────────────────────────
  if (y + 10 > pageH - 10) { doc.addPage(); y = 14; }
  y += 4;
  doc.setDrawColor(209, 213, 219);
  doc.setLineWidth(0.3);
  doc.line(mL, y, mL + cW, y);
  y += 5;
  doc.setFontSize(8);
  doc.setFont('helvetica', 'normal');
  doc.setTextColor(120, 120, 120);
  doc.text(
    `${questions.length} pergunta(s)   •   Score: ${evaluation.scoreTotal}/${evaluation.pesoTotal}   •   ${evaluation.classificacao}`,
    mL + cW / 2,
    y,
    { align: 'center' }
  );

  return doc;
}

export function generateEvaluationPdfBlobUrl(evaluation: Evaluation, form?: Formulario): string {
  const doc = buildEvalPdfDoc(evaluation, form);
  return URL.createObjectURL(doc.output('blob'));
}

export function exportEvaluationToPdf(evaluation: Evaluation, form?: Formulario): void {
  const doc = buildEvalPdfDoc(evaluation, form);
  const name = evaluation.patientNome.replace(/[^a-zA-Z0-9\-_]/g, '_');
  doc.save(`avaliacao-${name}.pdf`);
}
