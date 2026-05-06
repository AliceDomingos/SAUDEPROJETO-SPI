using SPI.Application.DTOs.Auth;
using SPI.Application.DTOs.Users;

namespace SPI.Application.Interfaces;

public interface IAuthAppService
{
    Task<TokenResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UserResponseDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponseDto> RegisterAsync(CreateUserRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task SendPasswordInviteAsync(Guid targetUserId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task SetPasswordFromInviteAsync(SetPasswordFromInviteRequestDto request, CancellationToken cancellationToken = default);
}



