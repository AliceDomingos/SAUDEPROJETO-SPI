using System.ComponentModel.DataAnnotations;

namespace SPI.Application.DTOs.Auth;

public sealed class SetPasswordFromInviteRequestDto
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;
}



