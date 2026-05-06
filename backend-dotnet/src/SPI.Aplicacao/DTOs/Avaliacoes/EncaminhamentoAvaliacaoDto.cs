namespace SPI.Application.DTOs.Evaluations;

public sealed class EvaluationReferralResponseDto
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

public sealed class SaveEvaluationReferralRequestDto
{
    public bool Encaminhado { get; init; }
    public Guid? SpecialistId { get; init; }
}
