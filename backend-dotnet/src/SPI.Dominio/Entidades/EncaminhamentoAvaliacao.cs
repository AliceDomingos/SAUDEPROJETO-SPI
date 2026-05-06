using SPI.Domain.Common;

namespace SPI.Domain.Entities;

public sealed class EvaluationReferral : Entity, IAggregateRoot
{
    private EvaluationReferral()
    {
    }

    public EvaluationReferral(
        Guid evaluationId,
        Guid patientId,
        Guid criadoPorUsuarioId,
        bool encaminhado,
        Guid? specialistId,
        string? specialistNome,
        string? especialidade,
        decimal custoEstimado)
    {
        if (evaluationId == Guid.Empty)
        {
            throw new InvalidOperationException("Avaliacao invalida.");
        }

        if (patientId == Guid.Empty)
        {
            throw new InvalidOperationException("Paciente invalido.");
        }

        if (criadoPorUsuarioId == Guid.Empty)
        {
            throw new InvalidOperationException("Usuario invalido.");
        }

        EvaluationId = evaluationId;
        PatientId = patientId;
        CriadoPorUsuarioId = criadoPorUsuarioId;
        CriadoEm = DateTime.UtcNow;

        UpdateDecision(encaminhado, specialistId, specialistNome, especialidade, custoEstimado);
    }

    public Guid EvaluationId { get; private set; }
    public Guid PatientId { get; private set; }
    public bool Encaminhado { get; private set; }
    public Guid? SpecialistId { get; private set; }
    public string? SpecialistNome { get; private set; }
    public string? Especialidade { get; private set; }
    public decimal CustoEstimado { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public Guid CriadoPorUsuarioId { get; private set; }
    public Guid? OrganizationId { get; private set; }

    public Evaluation Evaluation { get; private set; } = null!;
    public Patient Patient { get; private set; } = null!;
    public Specialist? Specialist { get; private set; }
    public User CriadoPorUsuario { get; private set; } = null!;
    public Organization? Organization { get; private set; }

    public void AssignOrganization(Guid organizationId) => OrganizationId = organizationId;

    public void UpdateDecision(bool encaminhado, Guid? specialistId, string? specialistNome, string? especialidade, decimal custoEstimado)
    {
        var normalizedSpecialistName = string.IsNullOrWhiteSpace(specialistNome) ? null : specialistNome.Trim();
        var normalizedSpecialty = string.IsNullOrWhiteSpace(especialidade) ? null : especialidade.Trim();

        if (encaminhado && (!specialistId.HasValue || specialistId.Value == Guid.Empty))
        {
            throw new InvalidOperationException("Especialista do encaminhamento e obrigatorio.");
        }

        if (encaminhado && string.IsNullOrWhiteSpace(normalizedSpecialistName))
        {
            throw new InvalidOperationException("Nome do especialista do encaminhamento e obrigatorio.");
        }

        if (encaminhado && string.IsNullOrWhiteSpace(normalizedSpecialty))
        {
            throw new InvalidOperationException("Especialidade do encaminhamento e obrigatoria.");
        }

        if (encaminhado && custoEstimado <= 0)
        {
            throw new InvalidOperationException("Custo estimado do encaminhamento deve ser maior que zero.");
        }

        Encaminhado = encaminhado;
        SpecialistId = encaminhado ? specialistId : null;
        SpecialistNome = encaminhado ? normalizedSpecialistName : null;
        Especialidade = encaminhado ? normalizedSpecialty : null;
        CustoEstimado = encaminhado ? custoEstimado : 0;
    }
}
