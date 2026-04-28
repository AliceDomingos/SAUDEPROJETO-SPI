using SPI.Domain.Common;
using SPI.Domain.Enums;
using SPI.Domain.ValueObjects;

namespace SPI.Domain.Entities;

public sealed class User : Entity, IAggregateRoot
{
    private readonly List<Patient> _pacientesCriados = [];
    private readonly List<Evaluation> _avaliacoesRealizadas = [];
    private readonly List<Group> _managedGroups = [];
    private readonly List<UserGroupMembership> _groupMemberships = [];

    private User()
    {
    }

    public User(string nome, Email email, string senhaHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do usuario e obrigatorio.");
        }

        Nome = nome.Trim();
        Email = email.Value;
        SenhaHash = senhaHash ?? string.Empty;
        Role = role;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.HealthAgent;
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public int? OrganizationId { get; private set; }

    public Organization? Organization { get; private set; }

    public void AssignOrganization(int organizationId) => OrganizationId = organizationId;

    public IReadOnlyCollection<Patient> PacientesCriados => _pacientesCriados;
    public IReadOnlyCollection<Evaluation> AvaliacoesRealizadas => _avaliacoesRealizadas;
    public IReadOnlyCollection<Group> ManagedGroups => _managedGroups;
    public IReadOnlyCollection<UserGroupMembership> GroupMemberships => _groupMemberships;

    public bool HasPasswordDefined() => !string.IsNullOrWhiteSpace(SenhaHash);

    public void Deactivate() => Ativo = false;

    public void UpdateProfile(string nome, Email email)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do usuario e obrigatorio.");
        }

        Nome = nome.Trim();
        Email = email.Value;
    }

    public void ChangeRole(UserRole role) => Role = role;

    public void DefinePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new InvalidOperationException("Hash de senha invalido.");
        }

        SenhaHash = passwordHash;
    }
}



