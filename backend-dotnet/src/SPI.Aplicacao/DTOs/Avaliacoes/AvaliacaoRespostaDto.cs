namespace SPI.Application.DTOs.Evaluations;

public sealed class EvaluationResponseDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientNome { get; init; } = string.Empty;
    public Guid AvaliadorId { get; init; }
    public string AvaliadorNome { get; init; } = string.Empty;
    public Guid GroupId { get; init; }
    public string GroupNome { get; init; } = string.Empty;
    public Guid? FormId { get; init; }
    public string? FormNome { get; init; }
    public IReadOnlyDictionary<string, int> Respostas { get; init; } = new Dictionary<string, int>();
    public decimal ScoreTotal { get; init; }
    public decimal PesoTotal { get; init; }
    public string Classificacao { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
    public DateTime DataAvaliacao { get; init; }
    public EvaluationReferralResponseDto? Referral { get; init; }
}



