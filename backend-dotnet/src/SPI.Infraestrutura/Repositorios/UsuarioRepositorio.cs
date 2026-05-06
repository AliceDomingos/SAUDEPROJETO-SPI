using SPI.Domain.Entities;
using SPI.Domain.Repositories;
using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SPI.Infrastructure.Data.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<User>> ListAsync(CancellationToken cancellationToken = default) =>
        _context.Users
            .AsNoTracking()
            .Include(x => x.GroupMemberships)
            .ThenInclude(x => x.Group)
            .Include(x => x.ManagedGroups)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<List<User>> ListByGroupIdsAsync(IReadOnlyCollection<Guid> groupIds, CancellationToken cancellationToken = default)
    {
        if (groupIds.Count == 0)
        {
            return Task.FromResult(new List<User>());
        }

        return _context.Users
            .AsNoTracking()
            .Include(x => x.GroupMemberships)
            .ThenInclude(x => x.Group)
            .Include(x => x.ManagedGroups)
            .Where(x =>
                x.GroupMemberships.Any(m => groupIds.Contains(m.GroupId)) ||
                x.ManagedGroups.Any(g => groupIds.Contains(g.Id)))
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<User>> ListByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        _context.Users
            .AsNoTracking()
            .Include(x => x.GroupMemberships)
            .ThenInclude(x => x.Group)
            .Include(x => x.ManagedGroups)
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Users
            .Include(x => x.GroupMemberships)
            .ThenInclude(x => x.Group)
            .Include(x => x.ManagedGroups)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _context.Users.FirstOrDefaultAsync(x => x.Email == normalized, cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        _context.Users.AddAsync(user, cancellationToken).AsTask();

    public async Task ReplaceGroupMembershipsAsync(
        Guid userId,
        IReadOnlyCollection<Guid> groupIds,
        CancellationToken cancellationToken = default)
    {
        var memberships = await _context.UserGroupMemberships
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.UserGroupMemberships.RemoveRange(memberships);

        foreach (var groupId in groupIds.Where(x => x != Guid.Empty).Distinct())
        {
            await _context.UserGroupMemberships.AddAsync(new UserGroupMembership(userId, groupId), cancellationToken);
        }
    }
}



