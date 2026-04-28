namespace SPI.Api.Extensions;

public sealed class DevelopmentUrlOptions
{
    public string? HttpUrl { get; init; }
    public string? HttpsUrl { get; init; }
}

public static class DevelopmentUrlConfiguration
{
    public static DevelopmentUrlOptions ConfigureDevelopmentUrls(this WebApplicationBuilder builder)
    {
        var configuredUrls = builder.Configuration["ASPNETCORE_URLS"] ?? builder.Configuration["urls"];
        var urls = SplitUrls(configuredUrls);

        if (urls.Count == 0)
        {
            return new DevelopmentUrlOptions();
        }

        if (builder.Environment.IsDevelopment())
        {
            DevelopmentPortProcessCleaner.StopProcessesUsingPorts(
                urls.Select(GetPortFromUrl).Where(x => x.HasValue).Select(x => x!.Value),
                Console.WriteLine);
        }

        builder.WebHost.UseUrls(string.Join(';', urls));

        return new DevelopmentUrlOptions
        {
            HttpUrl = urls.FirstOrDefault(x => x.StartsWith("http://", StringComparison.OrdinalIgnoreCase)),
            HttpsUrl = urls.FirstOrDefault(x => x.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        };
    }

    private static List<string> SplitUrls(string? urls) =>
        (urls ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static int? GetPortFromUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Port : null;
}
