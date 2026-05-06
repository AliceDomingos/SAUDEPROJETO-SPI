using SPI.Application.DTOs.Patients;

namespace SPI.Application.Interfaces;

public interface IPatientsAppService
{
    Task<IReadOnlyCollection<PatientResponseDto>> ListAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PatientResponseDto>> ListReusableAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<PatientResponseDto> CreateAsync(CreatePatientRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<PatientResponseDto> UpdateAsync(Guid id, UpdatePatientRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default);
}



