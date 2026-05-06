using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Specialists;

public sealed class CreateSpecialistRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Nome { get; init; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Especialidade { get; init; } = string.Empty;

    [Range(0.01, 999999.99)]
    public decimal CustoConsulta { get; init; }
}
