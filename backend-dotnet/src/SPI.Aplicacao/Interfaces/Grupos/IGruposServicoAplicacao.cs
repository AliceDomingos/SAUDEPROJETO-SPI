using SPI.Application.DTOs.Groups;

namespace SPI.Application.Interfaces;

public interface IGroupsAppService
{
    Task<IReadOnlyCollection<GroupResponseDto>> ListAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    Task<GroupResponseDto> CreateAsync(CreateGroupRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task<GroupResponseDto> UpdateAsync(Guid groupId, UpdateGroupRequestDto request, Guid actorUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid groupId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task AssignManagerAsync(Guid groupId, Guid managerId, CancellationToken cancellationToken = default);
}



