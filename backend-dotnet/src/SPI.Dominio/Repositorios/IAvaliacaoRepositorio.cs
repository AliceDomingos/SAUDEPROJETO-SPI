using SPI.Domain.Entities;
using SPI.Domain.ReadModels;

namespace SPI.Domain.Repositories;

public interface IEvaluationRepository
{
    Task<List<EvaluationDetails>> ListDetailedAsync(CancellationToken cancellationToken = default);
    Task<List<EvaluationDetails>> ListDetailedByGroupIdsAsync(IReadOnlyCollection<Guid> groupIds, CancellationToken cancellationToken = default);
    Task<List<EvaluationDetails>> ListDetailedByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<EvaluationDetails?> GetDetailedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AnyByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<bool> AnyByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<Evaluation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Evaluation?> GetByIdWithRelationsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Evaluation?> GetByIdWithReferralAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Evaluation evaluation, CancellationToken cancellationToken = default);
    Task AddReferralAsync(EvaluationReferral referral, CancellationToken cancellationToken = default);
    void Remove(Evaluation evaluation);
}


