using SPI.Api.Extensions;
using SPI.Application.DTOs.Groups;
using SPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPI.Api.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize(Policy = "GroupAccess")]
public sealed class GroupsController : ControllerBase
{
    private readonly IGroupsAppService _groupsAppService;

    public GroupsController(IGroupsAppService groupsAppService)
    {
        _groupsAppService = groupsAppService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _groupsAppService.ListAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "GroupManagement")]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _groupsAppService.CreateAsync(request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{groupId:guid}")]
    [Authorize(Policy = "GroupManagement")]
    public async Task<IActionResult> Update(Guid groupId, [FromBody] UpdateGroupRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _groupsAppService.UpdateAsync(groupId, request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{groupId:guid}")]
    [Authorize(Policy = "GroupManagement")]
    public async Task<IActionResult> Delete(Guid groupId, CancellationToken cancellationToken)
    {
        await _groupsAppService.DeleteAsync(groupId, User.GetUserId(), cancellationToken);
        return Ok(new { message = "Grupo excluido" });
    }
}



