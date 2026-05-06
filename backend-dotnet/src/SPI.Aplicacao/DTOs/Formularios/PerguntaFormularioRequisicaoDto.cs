using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Forms;

public sealed class FormQuestionRequestDto
{
    [Required]
    [MaxLength(1000)]
    public string Texto { get; init; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal Peso { get; init; }

    [Range(1, int.MaxValue)]
    public int Ordem { get; init; }

    public IReadOnlyCollection<FormQuestionOptionRequestDto> Opcoes { get; init; } = [];
}



