using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Forms;

public sealed class FormQuestionOptionRequestDto
{
    [Range(1, int.MaxValue)]
    public int Valor { get; init; }

    [Required]
    [MaxLength(500)]
    public string Descricao { get; init; } = string.Empty;
}
