namespace SPI.Domain.Entities;

public sealed class FormQuestion
{
    private FormQuestion()
    {
    }

    private readonly List<FormQuestionOption> _options = [];

    public FormQuestion(string texto, decimal peso, int ordem)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new InvalidOperationException("Texto da pergunta e obrigatorio.");
        }

        if (peso <= 0)
        {
            throw new InvalidOperationException("Peso da pergunta deve ser maior que zero.");
        }

        if (ordem <= 0)
        {
            throw new InvalidOperationException("Ordem da pergunta invalida.");
        }

        Texto = texto.Trim();
        Peso = peso;
        Ordem = ordem;
        Ativa = true;
    }

    public int Id { get; private set; }
    public int FormTemplateId { get; private set; }
    public string Texto { get; private set; } = string.Empty;
    public decimal Peso { get; private set; }
    public int Ordem { get; private set; }
    public bool Ativa { get; private set; }

    public FormTemplate FormTemplate { get; private set; } = null!;
    public IReadOnlyCollection<FormQuestionOption> Options => _options;

    internal void SetOptions(IEnumerable<(int Valor, string Descricao)> opcoes)
    {
        _options.Clear();
        foreach (var opcao in opcoes.OrderBy(o => o.Valor))
        {
            _options.Add(new FormQuestionOption(opcao.Valor, opcao.Descricao));
        }
    }
}



