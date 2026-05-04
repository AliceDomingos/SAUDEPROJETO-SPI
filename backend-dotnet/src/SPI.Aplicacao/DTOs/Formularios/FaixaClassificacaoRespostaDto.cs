namespace SPI.Application.DTOs.Forms;

public sealed class FormClassificationRangeResponseDto
{
    public decimal ScoreMin { get; init; }
    public decimal ScoreMax { get; init; }
    public string Rotulo { get; init; } = string.Empty;
}
