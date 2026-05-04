using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Forms;

public sealed class CreateFormRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Nome { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Descricao { get; init; }

    public int? GroupId { get; init; }

    [Required]
    public IReadOnlyCollection<FormQuestionRequestDto> Perguntas { get; init; } = [];

    public IReadOnlyCollection<FormClassificationRangeRequestDto> Faixas { get; init; } = [];
}



