using SPI.Domain.Entities;

namespace SPI.Domain.Repositories;

public interface IGroupRepository
{
    Task<List<Group>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<Group>> ListByIdsAsync(IReadOnlyCollection<Guid> groupIds, CancellationToken cancellationToken = default);
    Task<List<Group>> ListByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Group?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Group group, CancellationToken cancellationToken = default);
    void Remove(Group group);
}



