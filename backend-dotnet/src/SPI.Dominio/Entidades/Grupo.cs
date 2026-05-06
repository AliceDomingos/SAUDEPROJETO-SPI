using SPI.Domain.Common;

namespace SPI.Domain.Entities;

public sealed class Group : Entity, IAggregateRoot
{
    private readonly List<UserGroupMembership> _members = [];
    private readonly List<Patient> _patients = [];
    private readonly List<FormTemplate> _forms = [];

    private Group()
    {
    }

    public Group(string nome, Guid gestorId)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do grupo e obrigatorio.");
        }

        if (gestorId == Guid.Empty)
        {
            throw new InvalidOperationException("Gestor invalido.");
        }

        Nome = nome.Trim();
        GestorId = gestorId;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;
    public Guid GestorId { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public Guid? OrganizationId { get; private set; }

    public User Gestor { get; private set; } = null!;
    public Organization? Organization { get; private set; }

    public void AssignOrganization(Guid organizationId) => OrganizationId = organizationId;
    public IReadOnlyCollection<UserGroupMembership> Members => _members;
    public IReadOnlyCollection<Patient> Patients => _patients;
    public IReadOnlyCollection<FormTemplate> Forms => _forms;

    public void Update(string nome, Guid gestorId)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do grupo e obrigatorio.");
        }

        if (gestorId == Guid.Empty)
        {
            throw new InvalidOperationException("Gestor invalido.");
        }

        Nome = nome.Trim();
        GestorId = gestorId;
    }

    public void Deactivate() => Ativo = false;
}


