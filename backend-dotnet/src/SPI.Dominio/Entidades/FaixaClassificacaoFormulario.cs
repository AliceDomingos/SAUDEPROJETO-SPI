namespace SPI.Domain.Entities;

public sealed class FormClassificationRange
{
    private FormClassificationRange()
    {
    }

    public FormClassificationRange(decimal scoreMin, decimal scoreMax, string rotulo)
    {
        if (string.IsNullOrWhiteSpace(rotulo))
        {
            throw new InvalidOperationException("Rotulo da faixa e obrigatorio.");
        }

        if (scoreMax < scoreMin)
        {
            throw new InvalidOperationException("ScoreMax deve ser maior ou igual ao ScoreMin.");
        }

        ScoreMin = scoreMin;
        ScoreMax = scoreMax;
        Rotulo = rotulo.Trim();
    }

    public int Id { get; private set; }
    public int FormTemplateId { get; private set; }
    public decimal ScoreMin { get; private set; }
    public decimal ScoreMax { get; private set; }
    public string Rotulo { get; private set; } = string.Empty;

    public FormTemplate FormTemplate { get; private set; } = null!;
}
