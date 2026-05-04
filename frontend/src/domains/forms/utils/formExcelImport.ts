import * as XLSX from 'xlsx';
import type { FormImportData } from './formExport';

export function parseExcelForm(file: File): Promise<FormImportData> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const buffer = e.target?.result as ArrayBuffer;
        const wb = XLSX.read(new Uint8Array(buffer), { type: 'array' });
        const ws = wb.Sheets[wb.SheetNames[0]];
        const rows = XLSX.utils.sheet_to_json<Record<string, unknown>>(ws);

        if (rows.length === 0) throw new Error('Planilha vazia.');

        const nome = String(rows[0]['nome'] ?? '').trim() || 'Formulário importado';
        const descricaoRaw = String(rows[0]['descricao'] ?? '').trim();
        const descricao = descricaoRaw || undefined;

        // Detecta quantas colunas opcao_N_* existem nas headers
        const headers = Object.keys(rows[0]);
        const maxOpcoes = headers.reduce((max, h) => {
          const m = h.match(/^opcao_(\d+)_valor$/);
          return m ? Math.max(max, parseInt(m[1], 10)) : max;
        }, 0);

        const perguntas: FormImportData['perguntas'] = [];

        for (const row of rows) {
          const ordem = Number(row['pergunta_ordem']);
          const texto = String(row['pergunta_texto'] ?? '').trim();
          if (!texto || isNaN(ordem) || ordem <= 0) continue;

          const opcoes: Array<{ valor: number; descricao: string }> = [];
          for (let i = 1; i <= maxOpcoes; i++) {
            const val = row[`opcao_${i}_valor`];
            const desc = String(row[`opcao_${i}_descricao`] ?? '').trim();
            if (val !== '' && val !== undefined && val !== null) {
              opcoes.push({ valor: Number(val), descricao: desc });
            }
          }

          const peso = opcoes.length > 0
            ? Math.max(...opcoes.map((o) => o.valor))
            : Number(row['pergunta_peso'] ?? 0);

          perguntas.push({ texto, peso, ordem, opcoes });
        }

        if (perguntas.length === 0) throw new Error('Nenhuma pergunta encontrada na planilha.');

        const faixas: FormImportData['faixas'] = [];
        const faixasSheetName = wb.SheetNames.find((n) => n.toLowerCase() === 'faixas');
        if (faixasSheetName) {
          const wsFaixas = wb.Sheets[faixasSheetName];
          const faixaRows = XLSX.utils.sheet_to_json<Record<string, unknown>>(wsFaixas);
          for (const row of faixaRows) {
            const scoreMin = Number(row['score_min'] ?? 0);
            const scoreMax = Number(row['score_max'] ?? 0);
            const rotulo = String(row['rotulo'] ?? '').trim();
            if (rotulo) faixas.push({ scoreMin, scoreMax, rotulo });
          }
        }

        resolve({ nome, descricao, perguntas, faixas: faixas.length > 0 ? faixas : undefined });
      } catch (err) {
        reject(err instanceof Error ? err : new Error('Erro ao ler arquivo Excel.'));
      }
    };
    reader.onerror = () => reject(new Error('Erro ao ler o arquivo.'));
    reader.readAsArrayBuffer(file);
  });
}
