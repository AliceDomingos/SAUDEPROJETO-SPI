namespace SPI.Domain.Entities;

public sealed class FormQuestionOption
{
    private FormQuestionOption()
    {
    }

    public FormQuestionOption(int valor, string descricao)
    {
        if (valor <= 0)
        {
            throw new InvalidOperationException("Valor da opcao deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new InvalidOperationException("Descricao da opcao e obrigatoria.");
        }

        Valor = valor;
        Descricao = descricao.Trim();
    }

    public int Id { get; private set; }
    public int FormQuestionId { get; private set; }
    public int Valor { get; private set; }
    public string Descricao { get; private set; } = string.Empty;

    public FormQuestion FormQuestion { get; private set; } = null!;
}
