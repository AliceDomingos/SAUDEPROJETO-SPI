using SPI.Application.DTOs.Groups;

namespace SPI.Application.Interfaces;

public interface IGroupsAppService
{
    Task<IReadOnlyCollection<GroupResponseDto>> ListAsync(int actorUserId, CancellationToken cancellationToken = default);
    Task<GroupResponseDto> CreateAsync(CreateGroupRequestDto request, int actorUserId, CancellationToken cancellationToken = default);
    Task<GroupResponseDto> UpdateAsync(int groupId, UpdateGroupRequestDto request, int actorUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int groupId, int actorUserId, CancellationToken cancellationToken = default);
    Task AssignManagerAsync(int groupId, int managerId, CancellationToken cancellationToken = default);
}



