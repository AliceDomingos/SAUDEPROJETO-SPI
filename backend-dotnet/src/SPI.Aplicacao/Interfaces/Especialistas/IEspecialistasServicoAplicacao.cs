using SPI.Application.DTOs.Specialists;

namespace SPI.Application.Interfaces;

public interface ISpecialistsAppService
{
    Task<IReadOnlyCollection<SpecialistResponseDto>> ListAsync(Guid actorUserId, bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<SpecialistResponseDto> CreateAsync(CreateSpecialistRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<SpecialistResponseDto> UpdateAsync(Guid id, UpdateSpecialistRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
}
