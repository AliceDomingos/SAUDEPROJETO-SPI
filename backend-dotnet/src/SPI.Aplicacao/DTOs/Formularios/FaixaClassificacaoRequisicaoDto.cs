using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Forms;

public sealed class FormClassificationRangeRequestDto
{
    [Range(0, 999999)]
    public decimal ScoreMin { get; init; }

    [Range(0, 999999)]
    public decimal ScoreMax { get; init; }

    [Required]
    [MaxLength(200)]
    public string Rotulo { get; init; } = string.Empty;
}
