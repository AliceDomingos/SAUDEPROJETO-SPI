using SPI.Domain.Common;
using SPI.Domain.Services;

namespace SPI.Domain.Entities;

public sealed class Evaluation : Entity, IAggregateRoot
{
    private Evaluation()
    {
    }

    public Evaluation(Guid patientId, Guid avaliadorId, Guid groupId, Dictionary<string, int> respostas, string? observacoes = null)
    {
        if (patientId == Guid.Empty)
        {
            throw new InvalidOperationException("Paciente invalido.");
        }

        if (avaliadorId == Guid.Empty)
        {
            throw new InvalidOperationException("Avaliador invalido.");
        }

        if (groupId == Guid.Empty)
        {
            throw new InvalidOperationException("Grupo invalido.");
        }

        PatientId = patientId;
        AvaliadorId = avaliadorId;
        GroupId = groupId;
        Respostas = respostas ?? throw new InvalidOperationException("Respostas obrigatorias.");
        ScoreTotal = SPIClassificationService.CalculateScore(Respostas);
        PesoTotal = Respostas.Count;
        Classificacao = SPIClassificationService.Classify(ScoreTotal);
        Observacoes = NormalizeObservations(observacoes);
        DataAvaliacao = DateTime.UtcNow;
    }

    public Evaluation(
        int patientId,
        int avaliadorId,
        int groupId,
        int formTemplateId,
        Dictionary<int, int> respostas,
        IReadOnlyCollection<FormQuestion> questions,
        IReadOnlyCollection<FormClassificationRange> classificationRanges)
         string? observacoes = null)
    {
        if (patientId == Guid.Empty)
        {
            throw new InvalidOperationException("Paciente invalido.");
        }

        if (avaliadorId == Guid.Empty)
        {
            throw new InvalidOperationException("Avaliador invalido.");
        }

        if (groupId == Guid.Empty)
        {
            throw new InvalidOperationException("Grupo invalido.");
        }

        if (formTemplateId == Guid.Empty)
        {
            throw new InvalidOperationException("Formulario invalido.");
        }

        if (questions.Count == 0)
        {
            throw new InvalidOperationException("Formulario sem perguntas.");
        }

        var validQuestionIds = questions.Select(x => x.Id.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var invalidQuestionIds = respostas.Keys.Where(x => !validQuestionIds.Contains(x)).ToList();
        if (invalidQuestionIds.Count != 0)
        {
            throw new InvalidOperationException("As respostas contem perguntas invalidas.");
        }

        PatientId = patientId;
        AvaliadorId = avaliadorId;
        GroupId = groupId;
        FormTemplateId = formTemplateId;
        Respostas = respostas ?? throw new InvalidOperationException("Respostas obrigatorias.");
        PesoTotal = questions.Sum(x => x.Peso);
        ScoreTotal = Math.Round(
            questions.Where(x => Respostas.ContainsKey(x.Id.ToString())).Sum(x => x.Peso * Respostas[x.Id.ToString()]),
            2,
            MidpointRounding.AwayFromZero);
        Classificacao = "formulario";
        Observacoes = NormalizeObservations(observacoes);
        DataAvaliacao = DateTime.UtcNow;
    }

    public Guid PatientId { get; private set; }
    public Guid AvaliadorId { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid? FormTemplateId { get; private set; }
    public Dictionary<string, int> Respostas { get; private set; } = [];
    public decimal ScoreTotal { get; private set; }
    public decimal PesoTotal { get; private set; }
    public string Classificacao { get; private set; } = string.Empty;
    public string? Observacoes { get; private set; }
    public DateTime DataAvaliacao { get; private set; }
    public Guid? OrganizationId { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public User Avaliador { get; private set; } = null!;
    public Group Group { get; private set; } = null!;
    public FormTemplate? FormTemplate { get; private set; }
    public EvaluationReferral? Referral { get; private set; }
    public Organization? Organization { get; private set; }

    public void AssignOrganization(Guid organizationId) => OrganizationId = organizationId;

    private static string? NormalizeObservations(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
