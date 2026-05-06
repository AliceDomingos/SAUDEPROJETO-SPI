using SPI.Domain.Entities;

namespace SPI.Domain.Repositories;

public interface IUserRepository
{
    Task<List<User>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<User>> ListByGroupIdsAsync(IReadOnlyCollection<Guid> groupIds, CancellationToken cancellationToken = default);
    Task<List<User>> ListByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task ReplaceGroupMembershipsAsync(Guid userId, IReadOnlyCollection<Guid> groupIds, CancellationToken cancellationToken = default);
}



