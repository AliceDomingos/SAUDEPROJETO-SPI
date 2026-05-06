using SPI.Domain.Entities;

namespace SPI.Domain.Repositories;

public interface IPatientRepository
{
    Task<List<Patient>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<Patient>> ListByGroupIdsAsync(IReadOnlyCollection<Guid> groupIds, CancellationToken cancellationToken = default);
    Task<List<Patient>> ListByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<List<Patient>> ListReusableByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
    void Remove(Patient patient);
}


