using SPI.Application.DTOs.Users;

namespace SPI.Application.Interfaces;

public interface IUsersAppService
{
    Task<IReadOnlyCollection<UserResponseDto>> ListAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid userId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task UpdateGroupsAsync(Guid userId, UpdateUserGroupsRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
}



