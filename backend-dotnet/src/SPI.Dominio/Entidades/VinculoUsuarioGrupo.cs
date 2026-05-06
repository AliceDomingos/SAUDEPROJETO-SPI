namespace SPI.Domain.Entities;

public sealed class UserGroupMembership
{
    private UserGroupMembership()
    {
    }

    public UserGroupMembership(Guid userId, Guid groupId)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidOperationException("Usuario invalido.");
        }

        if (groupId == Guid.Empty)
        {
            throw new InvalidOperationException("Grupo invalido.");
        }

        UserId = userId;
        GroupId = groupId;
        CriadoEm = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid GroupId { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public User User { get; private set; } = null!;
    public Group Group { get; private set; } = null!;
}



