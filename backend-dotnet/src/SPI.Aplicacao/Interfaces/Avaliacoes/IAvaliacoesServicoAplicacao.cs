using SPI.Application.DTOs.Evaluations;

namespace SPI.Application.Interfaces;

public interface IEvaluationsAppService
{
    Task<IReadOnlyCollection<EvaluationResponseDto>> ListAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<EvaluationResponseDto?> GetByIdAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<EvaluationResponseDto> CreateAsync(CreateEvaluationRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<EvaluationReferralResponseDto> SaveReferralAsync(Guid id, SaveEvaluationReferralRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<EvaluationStatsResponseDto> GetStatsAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<ExportFileResultDto> ExportExcelAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<ExportFileResultDto> ExportPdfAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
}



