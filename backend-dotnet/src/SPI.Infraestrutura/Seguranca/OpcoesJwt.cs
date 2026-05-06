namespace SPI.Infrastructure.Data.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "SPI.Api";
    public string Audience { get; init; } = "SPI.React";
    public string SecretKey { get; init; } = "super-secret-key-change-in-production";
    public int ExpireMinutes { get; init; } = 1440;
}



