using SPI.Domain.Common;

namespace SPI.Domain.Entities;

public sealed class FormTemplate : Entity, IAggregateRoot
{
    private readonly List<FormQuestion> _questions = [];

    private FormTemplate()
    {
    }

    public FormTemplate(
        string nome,
        string? descricao,
        int criadoPorUsuarioId,
        int? groupId,
        IEnumerable<(string Texto, decimal Peso, int Ordem)> questions)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do formulario e obrigatorio.");
        }

        if (criadoPorUsuarioId <= 0)
        {
            throw new InvalidOperationException("Usuario criador invalido.");
        }

        Nome = nome.Trim();
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        CriadoPorUsuarioId = criadoPorUsuarioId;
        GroupId = groupId;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;

        ReplaceQuestions(questions);
    }

    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int? GroupId { get; private set; }
    public int CriadoPorUsuarioId { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }
    public int? OrganizationId { get; private set; }

    public Group? Group { get; private set; }
    public User CriadoPorUsuario { get; private set; } = null!;
    public Organization? Organization { get; private set; }

    public void AssignOrganization(int organizationId) => OrganizationId = organizationId;
    public IReadOnlyCollection<FormQuestion> Questions => _questions;
    public decimal PesoTotal => _questions.Sum(x => x.Peso);

    public void Update(
        string nome,
        string? descricao,
        int? groupId,
        IEnumerable<(string Texto, decimal Peso, int Ordem)> questions)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do formulario e obrigatorio.");
        }

        Nome = nome.Trim();
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        GroupId = groupId;
        AtualizadoEm = DateTime.UtcNow;

        ReplaceQuestions(questions);
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    private void ReplaceQuestions(IEnumerable<(string Texto, decimal Peso, int Ordem)> questions)
    {
        var list = questions
            .OrderBy(x => x.Ordem)
            .ToList();

        if (list.Count == 0)
        {
            throw new InvalidOperationException("O formulario deve ter pelo menos uma pergunta.");
        }

        _questions.Clear();
        foreach (var question in list)
        {
            _questions.Add(new FormQuestion(question.Texto, question.Peso, question.Ordem));
        }
    }
}



