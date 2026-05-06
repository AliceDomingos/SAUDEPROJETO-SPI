using System.Globalization;
using System.Text;
using SPI.Application.DTOs.Evaluations;

namespace SPI.Application.Services;

internal static class EvaluationExportBuilder
{
    public static ExportFileResultDto BuildCsvFile(EvaluationResponseDto evaluation) => new()
    {
        Content = Encoding.UTF8.GetBytes(BuildCsv(evaluation)),
        ContentType = "text/csv; charset=utf-8",
        FileName = $"avaliacao-{evaluation.Id}.csv"
    };

    public static ExportFileResultDto BuildPdfFile(EvaluationResponseDto evaluation) => new()
    {
        Content = SimplePdfDocument.Create(BuildLines(evaluation)),
        ContentType = "application/pdf",
        FileName = $"avaliacao-{evaluation.Id}.pdf"
    };

    private static string BuildCsv(EvaluationResponseDto evaluation)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Campo;Valor");
        builder.AppendLine($"Id;{evaluation.Id}");
        builder.AppendLine($"PacienteId;{evaluation.PatientId}");
        builder.AppendLine($"Paciente;{Escape(evaluation.PatientNome)}");
        builder.AppendLine($"AvaliadorId;{evaluation.AvaliadorId}");
        builder.AppendLine($"Avaliador;{Escape(evaluation.AvaliadorNome)}");
        builder.AppendLine($"GrupoId;{evaluation.GroupId}");
        builder.AppendLine($"Grupo;{Escape(evaluation.GroupNome)}");
        builder.AppendLine($"FormularioId;{evaluation.FormId}");
        builder.AppendLine($"Formulario;{Escape(evaluation.FormNome ?? string.Empty)}");
        builder.AppendLine($"ScoreTotal;{evaluation.ScoreTotal.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine($"PesoTotal;{evaluation.PesoTotal.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine($"Classificacao;{Escape(evaluation.Classificacao)}");
        builder.AppendLine($"Observacoes;{Escape(evaluation.Observacoes ?? string.Empty)}");
        builder.AppendLine($"DataAvaliacao;{evaluation.DataAvaliacao:O}");
        builder.AppendLine();
        builder.AppendLine("PerguntaId;Resposta");

        foreach (var resposta in evaluation.Respostas.OrderBy(x => x.Key))
        {
            builder.AppendLine($"{resposta.Key};{resposta.Value}");
        }

        return builder.ToString();
    }

    private static IReadOnlyCollection<string> BuildLines(EvaluationResponseDto evaluation)
    {
        var lines = new List<string>
        {
            $"Avaliacao #{evaluation.Id}",
            $"Paciente: {evaluation.PatientNome} ({evaluation.PatientId})",
            $"Avaliador: {evaluation.AvaliadorNome} ({evaluation.AvaliadorId})",
            $"Grupo: {evaluation.GroupNome} ({evaluation.GroupId})",
            $"Formulario: {evaluation.FormNome ?? "SPI padrao"}",
            $"Score Total: {evaluation.ScoreTotal.ToString(CultureInfo.InvariantCulture)}",
            $"Peso Total: {evaluation.PesoTotal.ToString(CultureInfo.InvariantCulture)}",
            $"Classificacao: {evaluation.Classificacao}",
            $"Observacoes: {evaluation.Observacoes ?? "Sem observacoes"}",
            $"Data: {evaluation.DataAvaliacao:dd/MM/yyyy HH:mm:ss}",
            "Respostas:"
        };

        lines.AddRange(evaluation.Respostas
            .OrderBy(x => x.Key)
            .Select(x => $"Pergunta {x.Key}: {x.Value}"));

        return lines;
    }

    private static string Escape(string value) => value.Replace(";", ",");
}

internal static class SimplePdfDocument
{
    public static byte[] Create(IReadOnlyCollection<string> lines)
    {
        var contentBuilder = new StringBuilder();
        var currentY = 780;

        foreach (var line in lines.Take(40))
        {
            var escaped = EscapePdf(line);
            contentBuilder.AppendLine($"BT /F1 10 Tf 40 {currentY} Td ({escaped}) Tj ET");
            currentY -= 16;
        }

        var streamContent = contentBuilder.ToString();
        var objects = new[]
        {
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj",
            "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj",
            "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj",
            $"4 0 obj << /Length {Encoding.ASCII.GetByteCount(streamContent)} >> stream\n{streamContent}endstream endobj",
            "5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj"
        };

        var builder = new StringBuilder();
        builder.AppendLine("%PDF-1.4");

        var offsets = new List<int>();
        foreach (var pdfObject in objects)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.AppendLine(pdfObject);
        }

        var xrefStart = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.AppendLine($"xref\n0 {objects.Length + 1}");
        builder.AppendLine("0000000000 65535 f ");
        foreach (var offset in offsets)
        {
            builder.AppendLine($"{offset:D10} 00000 n ");
        }

        builder.AppendLine($"trailer << /Size {objects.Length + 1} /Root 1 0 R >>");
        builder.AppendLine("startxref");
        builder.AppendLine(xrefStart.ToString(CultureInfo.InvariantCulture));
        builder.Append("%%EOF");

        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string EscapePdf(string value) =>
        value
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
}



