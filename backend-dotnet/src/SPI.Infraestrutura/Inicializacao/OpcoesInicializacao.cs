namespace SPI.Infrastructure.Data.Seed;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public bool Enabled { get; init; } = true;
    public string AdminName { get; init; } = "Administrador";
    public string AdminEmail { get; init; } = "admin@spi.com";
    public string AdminPassword { get; init; } = "admin123";
}



