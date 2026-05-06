namespace SPI.Application.DTOs.Specialists;

public sealed class SpecialistResponseDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Especialidade { get; init; } = string.Empty;
    public decimal CustoConsulta { get; init; }
    public bool Ativo { get; init; }
    public DateTime CriadoEm { get; init; }
}
