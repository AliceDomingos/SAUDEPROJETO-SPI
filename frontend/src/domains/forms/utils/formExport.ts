import { jsPDF } from 'jspdf';
import * as XLSX from 'xlsx';
import type { Formulario } from '../types';

export interface FormImportData {
  nome: string;
  descricao?: string;
  perguntas: Array<{
    texto: string;
    peso: number;
    ordem: number;
    opcoes?: Array<{ valor: number; descricao: string }>;
  }>;
}

function buildPdfDoc(form: Formulario): jsPDF {
  const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });

  const mL = 14;
  const pageW = doc.internal.pageSize.getWidth();
  const pageH = doc.internal.pageSize.getHeight();
  const cW = pageW - mL * 2;
  let y = 14;

  // ── Cabeçalho ─────────────────────────────────────────────────────────
  doc.setFontSize(7);
  doc.setFont('helvetica', 'bold');
  doc.setTextColor(140, 140, 140);
  doc.text('FORMULÁRIO DE AVALIAÇÃO', mL, y);

  y += 5;
  doc.setFontSize(15);
  doc.setFont('helvetica', 'bold');
  doc.setTextColor(20, 20, 20);
  const titleLines = doc.splitTextToSize(form.nome, cW) as string[];
  doc.text(titleLines, mL, y);
  y += titleLines.length * 7 + 1;

  if (form.descricao) {
    doc.setFontSize(8.5);
    doc.setFont('helvetica', 'italic');
    doc.setTextColor(90, 90, 90);
    const descLines = doc.splitTextToSize(form.descricao, cW) as string[];
    doc.text(descLines, mL, y);
    y += descLines.length * 4.5 + 3;
  }

  // Barra de metadados
  doc.setFillColor(243, 244, 246);
  doc.setDrawColor(209, 213, 219);
  doc.setLineWidth(0.3);
  doc.rect(mL, y, cW, 7.5, 'FD');
  doc.setFontSize(7.5);
  doc.setFont('helvetica', 'normal');
  doc.setTextColor(80, 80, 80);
  const metaParts: string[] = [];
  if (form.groupNome) metaParts.push(`Grupo: ${form.groupNome}`);
  metaParts.push(`Criado por: ${form.criadoPorNome}`);
  metaParts.push(`${form.perguntas.length} pergunta(s)   •   Peso máximo: ${form.pesoTotal}`);
  doc.text(metaParts.join('     '), mL + 3, y + 4.8);
  y += 12;

  // ── Perguntas ──────────────────────────────────────────────────────────
  const numW = 14;
  const textX = mL + numW;

  form.perguntas.forEach((q, i) => {
    const hasOpcoes = q.opcoes && q.opcoes.length > 0;

    if (hasOpcoes) {
      // ── Pergunta com opções de resposta ───────────────────────────────
      const headerTextW = cW - numW - 5;
      const tituloLinhas = doc.splitTextToSize(q.texto, headerTextW) as string[];
      const headerH = Math.max(10, tituloLinhas.length * 5.5 + 4);

      // Área de seleção: círculo + valor + descrição
      const circleX = textX + 5.5;
      const valX    = textX + 12;
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

      // Borda externa do bloco
      doc.setDrawColor(209, 213, 219);
      doc.setLineWidth(0.3);
      doc.rect(mL, startY, cW, totalH, 'D');

      // Fundo cinza no cabeçalho
      doc.setFillColor(243, 244, 246);
      doc.rect(mL, startY, cW, headerH, 'F');

      // Divisória vertical no cabeçalho
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

      // Linha separadora entre cabeçalho e opções
      doc.setDrawColor(209, 213, 219);
      doc.setLineWidth(0.3);
      doc.line(mL, startY + headerH, mL + cW, startY + headerH);

      // Opções
      let oy = startY + headerH;
      q.opcoes.forEach((o, oi) => {
        const linhas = opcaoLinhasArr[oi];
        const optH = Math.max(7, linhas.length * 4.5 + 2.5);

        // Separador entre opções (exceto na última)
        if (oi > 0) {
          doc.setDrawColor(229, 231, 235);
          doc.setLineWidth(0.2);
          doc.line(textX, oy, mL + cW, oy);
        }

        // Divisória vertical na área das opções
        doc.setDrawColor(209, 213, 219);
        doc.setLineWidth(0.3);
        doc.line(textX, oy, textX, oy + optH);

        // Círculo de seleção
        doc.setDrawColor(160, 160, 160);
        doc.setLineWidth(0.4);
        doc.circle(circleX, oy + optH / 2, 2.2, 'D');

        // Valor da opção
        doc.setFontSize(8);
        doc.setFont('helvetica', 'bold');
        doc.setTextColor(60, 60, 60);
        doc.text(String(o.valor), valX, oy + optH / 2 + 1.2, { align: 'center' });

        // Descrição da opção
        doc.setFontSize(8.5);
        doc.setFont('helvetica', 'normal');
        doc.setTextColor(30, 30, 30);
        doc.text(linhas, optTxtX, oy + (optH - linhas.length * 4.5) / 2 + 4);

        oy += optH;
      });

      y = startY + totalH + 3;

    } else {
      // ── Pergunta simples (sem opções) — número | texto | peso ──────────
      const pesoW  = 22;
      const pesoX  = mL + cW - pesoW;
      const simpTW = cW - numW - pesoW - 4;

      const tituloLinhas = doc.splitTextToSize(q.texto, simpTW) as string[];
      const rowH = Math.max(12, tituloLinhas.length * 5.5 + 5);

      if (y + rowH > pageH - 16) { doc.addPage(); y = 14; }

      doc.setFillColor(255, 255, 255);
      doc.setDrawColor(209, 213, 219);
      doc.setLineWidth(0.3);
      doc.rect(mL, y, cW, rowH, 'FD');

      // Número
      doc.setFontSize(9);
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(100, 100, 100);
      doc.text(String(i + 1).padStart(2, '0'), mL + numW / 2, y + rowH / 2 + 1.5, { align: 'center' });

      doc.setDrawColor(209, 213, 219);
      doc.line(textX, y, textX, y + rowH);

      // Texto
      doc.setFontSize(9);
      doc.setFont('helvetica', 'normal');
      doc.setTextColor(20, 20, 20);
      doc.text(tituloLinhas, textX + 4, y + 5.5);

      doc.line(pesoX, y, pesoX, y + rowH);

      // Peso
      doc.setFontSize(6);
      doc.setFont('helvetica', 'bold');
      doc.setTextColor(130, 130, 130);
      doc.text('PESO', pesoX + pesoW / 2, y + 4.5, { align: 'center' });
      doc.setFontSize(13);
      doc.setTextColor(30, 30, 30);
      doc.text(String(q.peso), pesoX + pesoW / 2, y + rowH / 2 + 3, { align: 'center' });

      y += rowH + 2;
    }
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
    `${form.perguntas.length} pergunta(s)   •   Peso máximo total: ${form.pesoTotal}`,
    mL + cW / 2,
    y,
    { align: 'center' }
  );

  return doc;
}

export function generatePdfBlobUrl(form: Formulario): string {
  const doc = buildPdfDoc(form);
  const blob = doc.output('blob');
  return URL.createObjectURL(blob);
}

export function exportFormToPdf(form: Formulario): void {
  const doc = buildPdfDoc(form);
  doc.save(`${form.nome.replace(/[^a-zA-Z0-9\-_]/g, '_')}.pdf`);
}


export function exportFormToExcel(form: Formulario): void {
  const maxOpcoes = Math.max(0, ...form.perguntas.map((q) => q.opcoes?.length ?? 0));

  const rows = form.perguntas.map((q) => {
    const row: Record<string, string | number> = {
      nome: form.nome,
      descricao: form.descricao ?? '',
      pergunta_ordem: q.ordem,
      pergunta_texto: q.texto,
      pergunta_peso: q.opcoes && q.opcoes.length > 0 ? '' : q.peso,
    };

    for (let i = 0; i < maxOpcoes; i++) {
      const o = q.opcoes?.[i];
      row[`opcao_${i + 1}_valor`] = o ? o.valor : '';
      row[`opcao_${i + 1}_descricao`] = o ? o.descricao : '';
    }

    return row;
  });

  const ws = XLSX.utils.json_to_sheet(rows);
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, 'Formulário');
  XLSX.writeFile(wb, `${form.nome.replace(/[^a-zA-Z0-9\-_]/g, '_')}.xlsx`);
}

export function parseImportedJson(file: File): Promise<FormImportData> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = JSON.parse(e.target?.result as string) as FormImportData;
        if (typeof data.nome !== 'string' || !Array.isArray(data.perguntas)) {
          reject(new Error('Arquivo inválido: estrutura incorreta.'));
          return;
        }
        const perguntasValidas = data.perguntas.every((q) => {
          if (typeof q.texto !== 'string' || typeof q.peso !== 'number') return false;
          if (q.opcoes !== undefined) {
            if (!Array.isArray(q.opcoes)) return false;
            if (!q.opcoes.every((o) => typeof o.valor === 'number' && typeof o.descricao === 'string')) return false;
          }
          return true;
        });
        if (!perguntasValidas) {
          reject(new Error('Arquivo inválido: perguntas com formato incorreto.'));
          return;
        }
        resolve(data);
      } catch {
        reject(new Error('Arquivo inválido: não é um JSON válido.'));
      }
    };
    reader.onerror = () => reject(new Error('Erro ao ler o arquivo.'));
    reader.readAsText(file);
  });
}
