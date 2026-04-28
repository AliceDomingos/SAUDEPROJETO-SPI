using SPI.Domain.Entities;

namespace SPI.Domain.Repositories;

public interface IFormRepository
{
    Task<List<FormTemplate>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<FormTemplate>> ListByGroupIdsAsync(IReadOnlyCollection<int> groupIds, CancellationToken cancellationToken = default);
    Task<List<FormTemplate>> ListByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<FormTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FormTemplate?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(FormTemplate formTemplate, CancellationToken cancellationToken = default);
}



