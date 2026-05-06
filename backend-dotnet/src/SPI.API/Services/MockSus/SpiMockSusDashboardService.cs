using System.Globalization;
using System.Text;

namespace SPI.Api.Services;

public sealed class SpiMockSusDashboardService
{
    private const double CustoMedioConsultaEspecializada = 1000;
    private readonly IWebHostEnvironment _environment;

    public SpiMockSusDashboardService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<SpiMockSusDashboardResponse> GetAsync(
        string? risco = null,
        string? especialista = null,
        string? mes = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var csvPath = ResolveDataPath("SPI mockados.csv");
        var rows = await Task.Run(() => ReadCsvRows(csvPath), cancellationToken);
        rows = ApplyFilters(rows, risco, especialista, mes, dataInicio, dataFim);

        var scores = rows.Select(x => x.ScoreTriagem).Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        var waitingTimes = rows.Select(x => x.TempoEsperaDias).Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        var triageDates = rows.Select(x => x.DataTriagem).Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        var encaminhadosRows = rows.Where(x => Same(x.Encaminhado, "Sim")).ToArray();
        var consultasRealizadasRows = rows.Where(x => Same(x.StatusConsulta, "Realizada")).ToArray();
        var diagnosticosConfirmadosRows = rows.Where(x => Same(x.DiagnosticoConfirmado, "Sim")).ToArray();
        var tratamentosIniciadosRows = rows.Where(x => Same(x.IniciouTratamento, "Sim")).ToArray();

        var encaminhados = encaminhadosRows.Length;
        var consultasRealizadas = consultasRealizadasRows.Length;
        var diagnosticosConfirmados = diagnosticosConfirmadosRows.Length;
        var tratamentosIniciados = tratamentosIniciadosRows.Length;
        var consultasEvitadas = rows.Count - encaminhados;

        return new SpiMockSusDashboardResponse
        {
            Fonte = Path.GetFileName(csvPath),
            Aba = "CSV",
            AtualizadoEm = File.GetLastWriteTime(csvPath),
            TotalPacientes = rows.Select(x => x.IdPaciente).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            TotalTriagens = rows.Count,
            TriagensMesAtual = triageDates.Length == 0 ? 0 : rows.Count(x => x.DataTriagem?.Year == triageDates.Max().Year && x.DataTriagem?.Month == triageDates.Max().Month),
            ScoreMedio = Round(scores.DefaultIfEmpty(0).Average()),
            MenorScore = scores.Length == 0 ? 0 : scores.Min(),
            MaiorScore = scores.Length == 0 ? 0 : scores.Max(),
            Encaminhados = encaminhados,
            ConsultasAgendadas = rows.Count(x => Same(x.StatusConsulta, "Agendada")),
            ConsultasRealizadas = consultasRealizadas,
            ConsultasCanceladas = rows.Count(x => Same(x.StatusConsulta, "Cancelada")),
            DiagnosticosConfirmados = diagnosticosConfirmados,
            TratamentosIniciados = tratamentosIniciados,
            CasosSeveros = rows.Count(x => Same(x.NivelRisco, "Severo")),
            TempoMedioEsperaDias = Round(waitingTimes.DefaultIfEmpty(0).Average()),
            TempoMedioIntervencaoDias = Round(tratamentosIniciadosRows.Select(x => x.TempoEsperaDias).Where(x => x.HasValue).Select(x => x!.Value).DefaultIfEmpty(0).Average()),
            ConsultasEvitadas = consultasEvitadas,
            EconomiaFinanceiraEstimada = consultasEvitadas * CustoMedioConsultaEspecializada,
            CustoTotalEncaminhamentos = 0,
            CustoMedioConsultaEspecializada = CustoMedioConsultaEspecializada,
            TaxaEncaminhamento = Percentage(encaminhados, rows.Count),
            TaxaComparecimento = Percentage(consultasRealizadas, encaminhados),
            TaxaDiagnosticoConfirmado = Percentage(diagnosticosConfirmados, consultasRealizadas),
            TaxaEncaminhamentoAssertivo = Percentage(diagnosticosConfirmados, consultasRealizadas),
            TaxaTratamentoAposDiagnostico = Percentage(tratamentosIniciados, diagnosticosConfirmados),
            PeriodoTriagens = new PeriodoTriagensResponse
            {
                Inicio = triageDates.Length == 0 ? null : triageDates.Min(),
                Fim = triageDates.Length == 0 ? null : triageDates.Max()
            },
            DistribuicaoTriagensMensais = MonthlyDistribution(rows),
            DistribuicaoRisco = Distribution(rows, x => BlankAs(x.NivelRisco, "Sem classificacao"), ["Severo", "Moderado", "Leve", "Sem Sinais"]),
            DistribuicaoStatusConsulta = Distribution(encaminhadosRows, x => BlankAs(x.StatusConsulta, "Aguardando"), ["Realizada", "Agendada", "Aguardando", "Cancelada"]),
            DistribuicaoEspecialista = Distribution(encaminhadosRows, x => BlankAs(x.EspecialistaDestino, "Sem especialista"), take: 6),
            DistribuicaoDiagnostico = Distribution(rows.Where(x => !string.IsNullOrWhiteSpace(x.TipoDiagnostico)), x => x.TipoDiagnostico, take: 8),
            DistribuicaoTratamento = Distribution(rows.Where(x => !string.IsNullOrWhiteSpace(x.TipoTratamento)), x => x.TipoTratamento, take: 8),
            UltimasTriagens = rows
                .OrderByDescending(x => x.DataTriagem ?? DateTime.MinValue)
                .Take(6)
                .Select(x => new SpiMockSusTriagemResponse
                {
                    IdPaciente = x.IdPaciente,
                    NomeCrianca = x.NomeCrianca,
                    IdadeAnos = x.IdadeAnos,
                    Sexo = x.Sexo,
                    UbsOrigem = x.UbsOrigem,
                    DataTriagem = x.DataTriagem,
                    ScoreTriagem = x.ScoreTriagem,
                    NivelRisco = x.NivelRisco,
                    Encaminhado = Same(x.Encaminhado, "Sim"),
                    StatusConsulta = BlankAs(x.StatusConsulta, "Sem consulta"),
                    DiagnosticoConfirmado = BlankAs(x.DiagnosticoConfirmado, "Pendente")
                })
                .ToArray()
        };
    }

    private static List<SpiMockSusRow> ApplyFilters(IEnumerable<SpiMockSusRow> rows, string? risco, string? especialista, string? mes, DateTime? dataInicio, DateTime? dataFim)
    {
        var query = rows;

        if (!string.IsNullOrWhiteSpace(risco))
        {
            query = query.Where(x => Same(x.NivelRisco, risco));
        }

        if (!string.IsNullOrWhiteSpace(especialista))
        {
            query = query.Where(x => Same(x.EspecialistaDestino, especialista));
        }

        if (!string.IsNullOrWhiteSpace(mes))
        {
            query = query.Where(x => x.DataTriagem.HasValue && Same(MonthlyLabel(x.DataTriagem.Value), mes));
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(x => x.DataTriagem.HasValue && x.DataTriagem.Value.Date >= dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(x => x.DataTriagem.HasValue && x.DataTriagem.Value.Date <= dataFim.Value.Date);
        }

        return query.ToList();
    }

    private string ResolveDataPath(string fileName)
    {
        var directory = new DirectoryInfo(_environment.ContentRootPath);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Arquivo mockado do SPI nao encontrado.", fileName);
    }

    private static List<SpiMockSusRow> ReadCsvRows(string csvPath)
    {
        using var reader = new StreamReader(csvPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), detectEncodingFromByteOrderMarks: true);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return [];
        }

        var headers = ParseCsvLine(headerLine).Select(x => x.Trim()).ToArray();
        var rows = new List<SpiMockSusRow>();

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);
            var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < headers.Length; index++)
            {
                record[headers[index]] = index < values.Count ? values[index].Trim() : string.Empty;
            }

            rows.Add(SpiMockSusRow.From(record));
        }

        return rows;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var value = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var current = line[index];
            if (current == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (current == ',' && !inQuotes)
            {
                values.Add(value.ToString());
                value.Clear();
                continue;
            }

            value.Append(current);
        }

        values.Add(value.ToString());
        return values;
    }

    private static IReadOnlyCollection<DistributionItemResponse> MonthlyDistribution(IEnumerable<SpiMockSusRow> rows)
    {
        return rows
            .Where(x => x.DataTriagem.HasValue)
            .GroupBy(x => new DateTime(x.DataTriagem!.Value.Year, x.DataTriagem.Value.Month, 1))
            .OrderBy(x => x.Key)
            .Select(group => new DistributionItemResponse
            {
                Label = MonthlyLabel(group.Key),
                Value = group.Count()
            })
            .ToArray();
    }

    private static string MonthlyLabel(DateTime date)
    {
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        return culture.TextInfo.ToTitleCase(date.ToString("MMM/yy", culture).Replace(".", ""));
    }

    private static IReadOnlyCollection<DistributionItemResponse> Distribution(
        IEnumerable<SpiMockSusRow> rows,
        Func<SpiMockSusRow, string> selector,
        IReadOnlyCollection<string>? preferredOrder = null,
        int? take = null)
    {
        var query = rows
            .GroupBy(selector, StringComparer.OrdinalIgnoreCase)
            .Select(group => new DistributionItemResponse { Label = group.Key, Value = group.Count() })
            .ToArray();

        IEnumerable<DistributionItemResponse> ordered = preferredOrder is null
            ? query.OrderByDescending(x => x.Value).ThenBy(x => x.Label)
            : query.OrderBy(x =>
            {
                var index = preferredOrder
                    .Select((value, order) => new { value, order })
                    .FirstOrDefault(item => string.Equals(item.value, x.Label, StringComparison.OrdinalIgnoreCase))
                    ?.order;
                return index ?? int.MaxValue;
            }).ThenByDescending(x => x.Value);

        return take.HasValue ? ordered.Take(take.Value).ToArray() : ordered.ToArray();
    }

    private static string BlankAs(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static bool Same(string? value, string expected) => string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    private static double Round(double value) => Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private static double Percentage(int value, int total) => total <= 0 ? 0 : Round(value * 100.0 / total);

    private sealed record SpiMockSusRow
    {
        public string IdPaciente { get; init; } = string.Empty;
        public string NomeCrianca { get; init; } = string.Empty;
        public string Sexo { get; init; } = string.Empty;
        public int? IdadeAnos { get; init; }
        public string UbsOrigem { get; init; } = string.Empty;
        public DateTime? DataTriagem { get; init; }
        public double? ScoreTriagem { get; init; }
        public string NivelRisco { get; init; } = string.Empty;
        public string Encaminhado { get; init; } = string.Empty;
        public string EspecialistaDestino { get; init; } = string.Empty;
        public string StatusConsulta { get; init; } = string.Empty;
        public string DiagnosticoConfirmado { get; init; } = string.Empty;
        public string TipoDiagnostico { get; init; } = string.Empty;
        public string IniciouTratamento { get; init; } = string.Empty;
        public string TipoTratamento { get; init; } = string.Empty;
        public double? TempoEsperaDias { get; init; }

        public static SpiMockSusRow From(IReadOnlyDictionary<string, string> values)
        {
            return new SpiMockSusRow
            {
                IdPaciente = Get(values, "id_paciente"),
                NomeCrianca = Get(values, "nome_crianca"),
                Sexo = Get(values, "sexo"),
                IdadeAnos = GetInt(values, "idade_anos"),
                UbsOrigem = Get(values, "ubs_origem"),
                DataTriagem = GetDate(values, "data_triagem"),
                ScoreTriagem = GetDouble(values, "score_triagem"),
                NivelRisco = Get(values, "nivel_risco"),
                Encaminhado = Get(values, "encaminhado"),
                EspecialistaDestino = Get(values, "especialista_destino"),
                StatusConsulta = Get(values, "status_consulta"),
                DiagnosticoConfirmado = Get(values, "diagnostico_confirmado"),
                TipoDiagnostico = Get(values, "tipo_diagnostico"),
                IniciouTratamento = Get(values, "iniciou_tratamento"),
                TipoTratamento = Get(values, "tipo_tratamento"),
                TempoEsperaDias = GetDouble(values, "tempo_espera_dias")
            };
        }

        private static string Get(IReadOnlyDictionary<string, string> values, string key)
        {
            return values.TryGetValue(key, out var value) ? value.Trim() : string.Empty;
        }

        private static int? GetInt(IReadOnlyDictionary<string, string> values, string key)
        {
            return int.TryParse(Get(values, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
        }

        private static double? GetDouble(IReadOnlyDictionary<string, string> values, string key)
        {
            return double.TryParse(Get(values, key), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;
        }

        private static DateTime? GetDate(IReadOnlyDictionary<string, string> values, string key)
        {
            var raw = Get(values, key);
            return DateTime.TryParseExact(raw, "dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.None, out var value)
                ? value
                : null;
        }
    }
}

public sealed class SpiMockSusDashboardResponse
{
    public string Fonte { get; init; } = string.Empty;
    public string Aba { get; init; } = string.Empty;
    public DateTime AtualizadoEm { get; init; }
    public int TotalPacientes { get; init; }
    public int TotalTriagens { get; init; }
    public int TriagensMesAtual { get; init; }
    public double ScoreMedio { get; init; }
    public double MenorScore { get; init; }
    public double MaiorScore { get; init; }
    public int Encaminhados { get; init; }
    public int ConsultasAgendadas { get; init; }
    public int ConsultasRealizadas { get; init; }
    public int ConsultasCanceladas { get; init; }
    public int DiagnosticosConfirmados { get; init; }
    public int TratamentosIniciados { get; init; }
    public int CasosSeveros { get; init; }
    public double TempoMedioEsperaDias { get; init; }
    public double TempoMedioIntervencaoDias { get; init; }
    public int ConsultasEvitadas { get; init; }
    public double EconomiaFinanceiraEstimada { get; init; }
    public double CustoTotalEncaminhamentos { get; init; }
    public double CustoMedioConsultaEspecializada { get; init; }
    public double TaxaEncaminhamento { get; init; }
    public double TaxaComparecimento { get; init; }
    public double TaxaDiagnosticoConfirmado { get; init; }
    public double TaxaEncaminhamentoAssertivo { get; init; }
    public double TaxaTratamentoAposDiagnostico { get; init; }
    public PeriodoTriagensResponse PeriodoTriagens { get; init; } = new();
    public IReadOnlyCollection<DistributionItemResponse> DistribuicaoTriagensMensais { get; init; } = [];
    public IReadOnlyCollection<DistributionItemResponse> DistribuicaoRisco { get; init; } = [];
    public IReadOnlyCollection<DistributionItemResponse> DistribuicaoStatusConsulta { get; init; } = [];
    public IReadOnlyCollection<DistributionItemResponse> DistribuicaoEspecialista { get; init; } = [];
    public IReadOnlyCollection<DistributionItemResponse> DistribuicaoDiagnostico { get; init; } = [];
    public IReadOnlyCollection<DistributionItemResponse> DistribuicaoTratamento { get; init; } = [];
    public IReadOnlyCollection<SpiMockSusTriagemResponse> UltimasTriagens { get; init; } = [];
}

public sealed class PeriodoTriagensResponse
{
    public DateTime? Inicio { get; init; }
    public DateTime? Fim { get; init; }
}

public sealed class DistributionItemResponse
{
    public string Label { get; init; } = string.Empty;
    public int Value { get; init; }
}

public sealed class SpiMockSusTriagemResponse
{
    public string IdPaciente { get; init; } = string.Empty;
    public string NomeCrianca { get; init; } = string.Empty;
    public int? IdadeAnos { get; init; }
    public string Sexo { get; init; } = string.Empty;
    public string UbsOrigem { get; init; } = string.Empty;
    public DateTime? DataTriagem { get; init; }
    public double? ScoreTriagem { get; init; }
    public string NivelRisco { get; init; } = string.Empty;
    public bool Encaminhado { get; init; }
    public string StatusConsulta { get; init; } = string.Empty;
    public string DiagnosticoConfirmado { get; init; } = string.Empty;
}
