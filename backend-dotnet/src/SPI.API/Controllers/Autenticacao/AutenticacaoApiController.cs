using SPI.Api.Extensions;
using SPI.Application.DTOs.Auth;
using SPI.Application.DTOs.Users;
using SPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthAppService _authAppService;

    public AuthController(IAuthAppService authAppService)
    {
        _authAppService = authAppService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authAppService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await _authAppService.GetCurrentUserAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("register")]
    [Authorize(Policy = "UserManagement")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authAppService.RegisterAsync(request, User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("users/{userId:guid}/password-invite")]
    [Authorize(Policy = "UserManagement")]
    public async Task<IActionResult> SendPasswordInvite(Guid userId, CancellationToken cancellationToken)
    {
        await _authAppService.SendPasswordInviteAsync(userId, User.GetUserId(), cancellationToken);
        return Ok(new { message = "Convite enviado" });
    }

    [HttpPost("set-password")]
    [AllowAnonymous]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordFromInviteRequestDto request, CancellationToken cancellationToken)
    {
        await _authAppService.SetPasswordFromInviteAsync(request, cancellationToken);
        return Ok(new { message = "Senha definida com sucesso" });
    }
}



