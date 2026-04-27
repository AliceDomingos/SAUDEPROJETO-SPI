import type { Formulario, FormQuestion } from '../types';

export interface FormImportData {
  nome: string;
  descricao?: string;
  perguntas: Array<{ texto: string; peso: number; ordem: number }>;
}

export function exportFormToPdf(form: Formulario): void {
  const printWindow = window.open('', '_blank', 'width=800,height=700');
  if (!printWindow) {
    alert('Permita pop-ups para exportar o PDF.');
    return;
  }

  const rows = form.perguntas
    .map(
      (q, i) => `
      <tr>
        <td style="padding:8px 6px;border-bottom:1px solid #eee;color:#888;font-size:13px;vertical-align:top;width:32px">${i + 1}</td>
        <td style="padding:8px 6px;border-bottom:1px solid #eee;font-size:13px;vertical-align:top">${q.texto}</td>
        <td style="padding:8px 6px;border-bottom:1px solid #eee;font-size:13px;text-align:center;color:#555;vertical-align:top;width:60px">${q.peso}</td>
      </tr>`
    )
    .join('');

  printWindow.document.write(`<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8"/>
  <title>${form.nome}</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 40px; color: #222; }
    h1 { font-size: 20px; margin: 0 0 6px; }
    .meta { color: #666; font-size: 13px; margin-bottom: 24px; line-height: 1.6; }
    table { width: 100%; border-collapse: collapse; }
    th { background: #f5f5f5; padding: 8px 6px; font-size: 12px; text-align: left; border-bottom: 2px solid #ddd; text-transform: uppercase; letter-spacing: 0.05em; color: #555; }
    .footer { margin-top: 20px; font-size: 12px; color: #999; text-align: right; border-top: 1px solid #eee; padding-top: 10px; }
    @media print { body { margin: 20px; } button { display: none; } }
  </style>
</head>
<body>
  <h1>${form.nome}</h1>
  <div class="meta">
    ${form.descricao ? `<div>${form.descricao}</div>` : ''}
    ${form.groupNome ? `<div>Grupo: <strong>${form.groupNome}</strong></div>` : ''}
    <div>Criado por: ${form.criadoPorNome}</div>
  </div>
  <table>
    <thead>
      <tr>
        <th>#</th>
        <th>Pergunta</th>
        <th style="text-align:center">Peso</th>
      </tr>
    </thead>
    <tbody>${rows}</tbody>
  </table>
  <div class="footer">
    ${form.perguntas.length} pergunta(s) &nbsp;&bull;&nbsp; Peso total: <strong>${form.pesoTotal}</strong>
  </div>
</body>
</html>`);

  printWindow.document.close();
  printWindow.focus();
  setTimeout(() => {
    printWindow.print();
    printWindow.close();
  }, 300);
}

export function exportFormToJson(form: Formulario): void {
  const data: FormImportData = {
    nome: form.nome,
    descricao: form.descricao,
    perguntas: form.perguntas.map((q, i) => ({
      texto: q.texto,
      peso: q.peso,
      ordem: i + 1,
    })),
  };

  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `${form.nome.replace(/[^a-zA-Z0-9\-_]/g, '_')}.json`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
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
        const perguntasValidas = data.perguntas.every(
          (q) => typeof q.texto === 'string' && typeof q.peso === 'number'
        );
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
