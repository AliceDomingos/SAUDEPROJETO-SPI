using SPI.Api.Extensions;
using SPI.Application.DTOs.Users;
using SPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPI.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "UserManagement")]
public sealed class UsersController : ControllerBase
{
    private readonly IUsersAppService _usersAppService;

    public UsersController(IUsersAppService usersAppService)
    {
        _usersAppService = usersAppService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _usersAppService.ListAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{userId:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid userId, CancellationToken cancellationToken)
    {
        await _usersAppService.DeactivateAsync(userId, User.GetUserId(), cancellationToken);
        return Ok(new { message = "Usuario desativado" });
    }

    [HttpPut("{userId:guid}/groups")]
    public async Task<IActionResult> UpdateGroups(
        Guid userId,
        [FromBody] UpdateUserGroupsRequestDto request,
        CancellationToken cancellationToken)
    {
        await _usersAppService.UpdateGroupsAsync(userId, request, User.GetUserId(), cancellationToken);
        return Ok(new { message = "Grupos do usuario atualizados" });
    }
}



