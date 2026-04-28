using SPI.Domain.Entities;

namespace SPI.Domain.Repositories;

public interface IPatientRepository
{
    Task<List<Patient>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<Patient>> ListByGroupIdsAsync(IReadOnlyCollection<int> groupIds, CancellationToken cancellationToken = default);
    Task<List<Patient>> ListByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
    void Remove(Patient patient);
}



