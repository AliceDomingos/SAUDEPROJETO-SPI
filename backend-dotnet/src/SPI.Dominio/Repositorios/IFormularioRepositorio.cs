using SPI.Domain.Entities;

namespace SPI.Domain.Repositories;

public interface IFormRepository
{
    Task<List<FormTemplate>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<FormTemplate>> ListByGroupIdsAsync(IReadOnlyCollection<Guid> groupIds, CancellationToken cancellationToken = default);
    Task<List<FormTemplate>> ListByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<FormTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FormTemplate?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(FormTemplate formTemplate, CancellationToken cancellationToken = default);
}



