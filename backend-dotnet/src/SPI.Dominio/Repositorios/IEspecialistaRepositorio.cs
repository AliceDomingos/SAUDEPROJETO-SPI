using SPI.Domain.Entities;

namespace SPI.Domain.Repositories;

public interface ISpecialistRepository
{
    Task<List<Specialist>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<Specialist>> ListActiveAsync(CancellationToken cancellationToken = default);
    Task<List<Specialist>> ListByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<List<Specialist>> ListActiveByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Specialist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Specialist specialist, CancellationToken cancellationToken = default);
}
