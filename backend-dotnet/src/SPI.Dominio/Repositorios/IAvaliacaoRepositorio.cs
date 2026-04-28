using SPI.Domain.Entities;
using SPI.Domain.ReadModels;

namespace SPI.Domain.Repositories;

public interface IEvaluationRepository
{
    Task<List<EvaluationDetails>> ListDetailedAsync(CancellationToken cancellationToken = default);
    Task<List<EvaluationDetails>> ListDetailedByGroupIdsAsync(IReadOnlyCollection<int> groupIds, CancellationToken cancellationToken = default);
    Task<List<EvaluationDetails>> ListDetailedByOrganizationIdAsync(int organizationId, CancellationToken cancellationToken = default);
    Task<EvaluationDetails?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AnyByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<bool> AnyByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<Evaluation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Evaluation?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Evaluation evaluation, CancellationToken cancellationToken = default);
    void Remove(Evaluation evaluation);
}



