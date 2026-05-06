using SPI.Application.DTOs.Evaluations;

namespace SPI.Application.DTOs.Dashboard;

public sealed class DashboardResponseDto
{
    public int TotalUsuarios { get; init; }
    public int TotalPacientes { get; init; }
    public int TotalAvaliacoes { get; init; }
    public int TotalFormularios { get; init; }
    public int TotalGrupos { get; init; }
    public DashboardTriageSummaryDto Triagens { get; init; } = new();
    public IReadOnlyCollection<EvaluationResponseDto> UltimasAvaliacoes { get; init; } = [];
}

public sealed class DashboardTriageSummaryDto
{
    public int TotalPacientes { get; init; }
    public int TotalTriagens { get; init; }
    public decimal ScoreMedio { get; init; }
    public decimal MenorScore { get; init; }
    public decimal MaiorScore { get; init; }
    public int Encaminhados { get; init; }
    public int ConsultasEvitadas { get; init; }
    public decimal EconomiaFinanceiraEstimada { get; init; }
    public decimal CustoTotalEncaminhamentos { get; init; }
    public int CasosSeveros { get; init; }
    public decimal TaxaEncaminhamento { get; init; }
    public IReadOnlyCollection<DashboardDistributionItemDto> DistribuicaoTriagensMensais { get; init; } = [];
    public IReadOnlyCollection<DashboardDistributionItemDto> DistribuicaoRisco { get; init; } = [];
    public IReadOnlyCollection<DashboardDistributionItemDto> DistribuicaoEspecialista { get; init; } = [];
}

public sealed class DashboardDistributionItemDto
{
    public string Label { get; init; } = string.Empty;
    public int Value { get; init; }
}



