using SPI.Domain.Common;
using SPI.Domain.Services;

namespace SPI.Domain.Entities;

public sealed class Evaluation : Entity, IAggregateRoot
{
    private Evaluation()
    {
    }

    public Evaluation(int patientId, int avaliadorId, int groupId, Dictionary<int, int> respostas)
    {
        if (patientId <= 0)
        {
            throw new InvalidOperationException("Paciente invalido.");
        }

        if (avaliadorId <= 0)
        {
            throw new InvalidOperationException("Avaliador invalido.");
        }

        if (groupId <= 0)
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
        DataAvaliacao = DateTime.UtcNow;
    }

    public Evaluation(
        int patientId,
        int avaliadorId,
        int groupId,
        int formTemplateId,
        Dictionary<int, int> respostas,
        IReadOnlyCollection<FormQuestion> questions)
    {
        if (patientId <= 0)
        {
            throw new InvalidOperationException("Paciente invalido.");
        }

        if (avaliadorId <= 0)
        {
            throw new InvalidOperationException("Avaliador invalido.");
        }

        if (groupId <= 0)
        {
            throw new InvalidOperationException("Grupo invalido.");
        }

        if (formTemplateId <= 0)
        {
            throw new InvalidOperationException("Formulario invalido.");
        }

        if (questions.Count == 0)
        {
            throw new InvalidOperationException("Formulario sem perguntas.");
        }

        var invalidQuestionIds = respostas.Keys.Except(questions.Select(x => x.Id)).ToList();
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
            questions.Where(x => Respostas.ContainsKey(x.Id)).Sum(x => x.Peso * Respostas[x.Id]),
            2,
            MidpointRounding.AwayFromZero);
        Classificacao = "formulario";
        DataAvaliacao = DateTime.UtcNow;
    }

    public int PatientId { get; private set; }
    public int AvaliadorId { get; private set; }
    public int GroupId { get; private set; }
    public int? FormTemplateId { get; private set; }
    public Dictionary<int, int> Respostas { get; private set; } = [];
    public decimal ScoreTotal { get; private set; }
    public decimal PesoTotal { get; private set; }
    public string Classificacao { get; private set; } = string.Empty;
    public DateTime DataAvaliacao { get; private set; }
    public int? OrganizationId { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public User Avaliador { get; private set; } = null!;
    public Group Group { get; private set; } = null!;
    public FormTemplate? FormTemplate { get; private set; }
    public Organization? Organization { get; private set; }

    public void AssignOrganization(int organizationId) => OrganizationId = organizationId;
}



