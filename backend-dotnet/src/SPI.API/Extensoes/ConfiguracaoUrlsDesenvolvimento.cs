namespace SPI.Api.Extensions;

public sealed class DevelopmentUrlOptions
{
    public string? HttpUrl { get; init; }
    public string? HttpsUrl { get; init; }
}

public static class DevelopmentUrlConfiguration
{
    private static readonly string[] DefaultDevelopmentUrls =
    [
        "http://localhost:5080",
        "https://localhost:7060"
    ];

    public static DevelopmentUrlOptions ConfigureDevelopmentUrls(this WebApplicationBuilder builder)
    {
        var configuredUrls = builder.Configuration["ASPNETCORE_URLS"] ?? builder.Configuration["urls"];
        var urls = SplitUrls(configuredUrls);
        if (urls.Count == 0 && builder.Environment.IsDevelopment())
        {
            urls = GetLaunchSettingsUrls(builder.Environment.ContentRootPath);
        }

        if (builder.Environment.IsDevelopment())
        {
            IEnumerable<string> portsToRelease = urls.Count == 0
                ? DefaultDevelopmentUrls
                : urls;

            DevelopmentPortProcessCleaner.StopProcessesUsingPorts(
                portsToRelease.Select(GetPortFromUrl).Where(x => x.HasValue).Select(x => x!.Value),
                Console.WriteLine);
        }

        if (urls.Count == 0)
        {
            return new DevelopmentUrlOptions();
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

    private static List<string> GetLaunchSettingsUrls(string contentRootPath)
    {
        var launchSettingsPath = Path.Combine(contentRootPath, "Properties", "launchSettings.json");
        if (!File.Exists(launchSettingsPath))
        {
            return [];
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(launchSettingsPath));
            if (!document.RootElement.TryGetProperty("profiles", out var profiles))
            {
                return [];
            }

            var profileName = Environment.GetEnvironmentVariable("ASPNETCORE_LAUNCH_PROFILE");
            if (!string.IsNullOrWhiteSpace(profileName) &&
                profiles.TryGetProperty(profileName, out var selectedProfile) &&
                selectedProfile.TryGetProperty("applicationUrl", out var selectedUrls))
            {
                return SplitUrls(selectedUrls.GetString());
            }

            foreach (var profile in profiles.EnumerateObject())
            {
                if (profile.Value.TryGetProperty("applicationUrl", out var applicationUrls))
                {
                    var urls = SplitUrls(applicationUrls.GetString());
                    if (urls.Count > 0)
                    {
                        return urls;
                    }
                }
            }
        }
        catch
        {
            return [];
        }

        return [];
    }
}
