namespace SPI.Application.DTOs.Forms;

public sealed class FormResponseDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public int? GroupId { get; init; }
    public string? GroupNome { get; init; }
    public int CriadoPorUsuarioId { get; init; }
    public string CriadoPorNome { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public decimal PesoTotal { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime AtualizadoEm { get; init; }
    public IReadOnlyCollection<FormQuestionResponseDto> Perguntas { get; init; } = [];
    public IReadOnlyCollection<FormClassificationRangeResponseDto> Faixas { get; init; } = [];
}



