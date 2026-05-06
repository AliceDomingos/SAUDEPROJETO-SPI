namespace SPI.Domain.ReadModels;

public sealed class EvaluationDetails
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientNome { get; init; } = string.Empty;
    public Guid AvaliadorId { get; init; }
    public string AvaliadorNome { get; init; } = string.Empty;
    public Guid GroupId { get; init; }
    public string GroupNome { get; init; } = string.Empty;
    public Guid? FormTemplateId { get; init; }
    public string? FormNome { get; init; }
    public IReadOnlyDictionary<string, int> Respostas { get; init; } = new Dictionary<string, int>();
    public decimal ScoreTotal { get; init; }
    public decimal PesoTotal { get; init; }
    public string Classificacao { get; init; } = string.Empty;
    public string? Observacoes { get; init; }
    public DateTime DataAvaliacao { get; init; }
    public EvaluationReferralDetails? Referral { get; init; }
}

public sealed class EvaluationReferralDetails
{
    public Guid Id { get; init; }
    public Guid EvaluationId { get; init; }
    public Guid PatientId { get; init; }
    public bool Encaminhado { get; init; }
    public Guid? SpecialistId { get; init; }
    public string? SpecialistNome { get; init; }
    public string? Especialidade { get; init; }
    public decimal CustoEstimado { get; init; }
    public DateTime CriadoEm { get; init; }
}



