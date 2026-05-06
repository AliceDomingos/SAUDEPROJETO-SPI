using SPI.Domain.Entities;

namespace SPI.Domain.Repositories;

public interface IOrganizationRepository
{
    Task<Organization?> GetByAdminIdAsync(Guid adminId, CancellationToken cancellationToken = default);
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Organization organization, CancellationToken cancellationToken = default);
}
