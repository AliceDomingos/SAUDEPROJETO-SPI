using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SPI.Application.Configuration;
using SPI.Application.Interfaces.Seguranca;
using SPI.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SPI.Infrastructure.Data.Security;

public sealed class PasswordInviteTokenService : IPasswordInviteTokenService
{
    private const string PurposeClaim = "purpose";
    private const string PasswordSnapshotClaim = "pwd_snapshot";
    private const string PurposeValue = "password_invite";

    private readonly JwtOptions _jwtOptions;
    private readonly PasswordInviteOptions _inviteOptions;

    public PasswordInviteTokenService(
        IOptions<JwtOptions> jwtOptions,
        IOptions<PasswordInviteOptions> inviteOptions)
    {
        _jwtOptions = jwtOptions.Value;
        _inviteOptions = inviteOptions.Value;
    }

    public string Generate(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(PurposeClaim, PurposeValue),
            new(PasswordSnapshotClaim, user.SenhaHash ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_inviteOptions.ExpireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public PasswordInviteTokenPayload Validate(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var handler = new JwtSecurityTokenHandler();

        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        }, out _);

        var purpose = principal.FindFirstValue(PurposeClaim);
        if (!string.Equals(purpose, PurposeValue, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Token de convite invalido.");
        }

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
        {
            throw new InvalidOperationException("Token de convite invalido.");
        }

        var passwordHashSnapshot = principal.FindFirstValue(PasswordSnapshotClaim) ?? string.Empty;
        return new PasswordInviteTokenPayload(userId, passwordHashSnapshot);
    }
}



