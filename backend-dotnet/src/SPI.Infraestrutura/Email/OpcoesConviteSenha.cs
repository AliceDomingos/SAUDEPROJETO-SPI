namespace SPI.Infrastructure.Email;

public sealed class PasswordInviteOptions
{
    public const string SectionName = "PasswordInvite";

    public string FrontendBaseUrl { get; init; } = "http://localhost:5173";
    public int ExpireMinutes { get; init; } = 1440;
}



