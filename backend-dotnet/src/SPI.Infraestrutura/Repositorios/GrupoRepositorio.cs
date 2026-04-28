using SPI.Domain.Entities;
using SPI.Domain.Repositories;
using SPI.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SPI.Infrastructure.Data.Repositories;

public sealed class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Group>> ListAsync(CancellationToken cancellationToken = default) =>
        _context.Groups
            .AsNoTracking()
            .Include(x => x.Gestor)
            .Include(x => x.Members)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<List<Group>> ListByIdsAsync(IReadOnlyCollection<int> groupIds, CancellationToken cancellationToken = default)
    {
        if (groupIds.Count == 0)
        {
            return Task.FromResult(new List<Group>());
        }

        return _context.Groups
            .AsNoTracking()
            .Include(x => x.Gestor)
            .Include(x => x.Members)
            .Where(x => groupIds.Contains(x.Id))
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Group>> ListByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default) =>
        _context.Groups
            .AsNoTracking()
            .Include(x => x.Gestor)
            .Include(x => x.Members)
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

    public Task<Group?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Groups.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Group?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Groups
            .Include(x => x.Gestor)
            .Include(x => x.Members)
            .ThenInclude(x => x.User)
            .Include(x => x.Patients)
            .Include(x => x.Forms)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Group group, CancellationToken cancellationToken = default) =>
        _context.Groups.AddAsync(group, cancellationToken).AsTask();

    public void Remove(Group group) => _context.Groups.Remove(group);
}



