namespace SPI.Application.DTOs.Forms;

public sealed class FormQuestionResponseDto
{
    public Guid Id { get; init; }
    public string Texto { get; init; } = string.Empty;
    public decimal Peso { get; init; }
    public int Ordem { get; init; }
    public bool Ativa { get; init; }
    public IReadOnlyCollection<FormQuestionOptionResponseDto> Opcoes { get; init; } = [];
}

public sealed class FormQuestionOptionResponseDto
{
    public int Valor { get; init; }
    public string Descricao { get; init; } = string.Empty;
}



