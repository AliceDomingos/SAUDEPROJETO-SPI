using SPI.Application.DTOs.Profile;
using SPI.Application.DTOs.Users;

namespace SPI.Application.Interfaces;

public interface IProfileAppService
{
    Task<UserResponseDto> GetAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UpdateAsync(UpdateProfileRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
}



