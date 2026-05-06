using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Users;

public sealed class UpdateUserGroupsRequestDto
{
    [Required]
    public IReadOnlyCollection<Guid> GroupIds { get; init; } = [];
}



