using SPI.Domain.Common;

namespace SPI.Domain.Entities;

public sealed class Organization : Entity, IAggregateRoot
{
    private Organization()
    {
    }

    public Organization(string nome, Guid adminId)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome da organizacao e obrigatorio.");
        }

        if (adminId == Guid.Empty)
        {
            throw new InvalidOperationException("Admin invalido.");
        }

        Nome = nome.Trim();
        AdminId = adminId;
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;
    public Guid AdminId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public User Admin { get; private set; } = null!;

    public void Update(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome da organizacao e obrigatorio.");
        }

        Nome = nome.Trim();
    }
}
