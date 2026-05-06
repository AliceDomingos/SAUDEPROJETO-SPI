namespace SPI.Application.DTOs.Evaluations;

public sealed class EvaluationStatsResponseDto
{
    public int Total { get; init; }
    public decimal AverageScore { get; init; }
    public int LastMonth { get; init; }
    public ClassificationDistributionDto ClassificationDistribution { get; init; } = new();
    public IReadOnlyCollection<EvaluationResponseDto> RecentEvaluations { get; init; } = [];
}

public sealed class ClassificationDistributionDto
{
    public int SemIndicativo { get; init; }
    public int TeaLeveModerado { get; init; }
    public int TeaGrave { get; init; }
}



