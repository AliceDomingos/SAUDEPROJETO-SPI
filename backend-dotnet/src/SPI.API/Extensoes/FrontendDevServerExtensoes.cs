using System.Diagnostics;
using System.Net.NetworkInformation;

namespace SPI.Api.Extensions;

public static class FrontendDevServerExtensions
{
    private const int PreferredFrontendPort = 5174;
    private static readonly Lock StartLock = new();
    private static readonly Lock BrowserLock = new();
    private static bool _frontendStartAttempted;
    private static bool _browserOpenAttempted;

    public static void TryStartFrontendDevServer(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        lock (StartLock)
        {
            if (_frontendStartAttempted)
            {
                return;
            }

            _frontendStartAttempted = true;
        }

        var frontendPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "..", "frontend"));
        if (!Directory.Exists(frontendPath))
        {
            app.Logger.LogWarning("Pasta do frontend nao encontrada em {FrontendPath}.", frontendPath);
            return;
        }

        var developmentUrlOptions = app.Services.GetRequiredService<DevelopmentUrlOptions>();
        var portFilePath = Path.Combine(frontendPath, ".vite-port");
        var frontendPort = ResolveFrontendPort(portFilePath);
        var frontendUrl = $"http://localhost:{frontendPort}";

        DevelopmentPortProcessCleaner.StopProcessesUsingPorts(
            [frontendPort],
            message => app.Logger.LogInformation("{Message}", message));

        if (IsPortOccupied(frontendPort))
        {
            app.Logger.LogWarning("A porta fixa do frontend ({FrontendPort}) ainda esta ocupada.", frontendPort);
            return;
        }

        if (!File.Exists(Path.Combine(frontendPath, "package.json")))
        {
            app.Logger.LogWarning("package.json do frontend nao foi encontrado em {FrontendPath}.", frontendPath);
            return;
        }

        if (!Directory.Exists(Path.Combine(frontendPath, "node_modules")))
        {
            app.Logger.LogWarning("Dependencias do frontend nao instaladas. Execute 'npm install' em {FrontendPath}.", frontendPath);
            return;
        }

        try
        {
            var processStartInfo = OperatingSystem.IsWindows()
                ? new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c npx vite --host localhost --port {frontendPort}",
                    WorkingDirectory = frontendPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
                : new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-lc \"npx vite --host localhost --port {frontendPort}\"",
                    WorkingDirectory = frontendPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

            processStartInfo.Environment["BROWSER"] = "none";
            processStartInfo.Environment["VITE_API_URL"] = developmentUrlOptions.HttpUrl ?? "http://localhost:5080";

            var process = Process.Start(processStartInfo);
            if (process is null)
            {
                app.Logger.LogWarning("Nao foi possivel iniciar o frontend automaticamente.");
                return;
            }

            File.WriteAllText(portFilePath, frontendPort.ToString());
            _ = Task.Run(() => ForwardOutputAsync(process, app.Logger));
            _ = Task.Run(() => WaitForFrontendAsync(app.Logger, frontendUrl, frontendPort));

            app.Logger.LogInformation("Iniciando frontend React em {FrontendUrl}.", frontendUrl);
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Falha ao iniciar o frontend automaticamente.");
        }
    }

    private static async Task WaitForFrontendAsync(ILogger logger, string frontendUrl, int frontendPort)
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            if (IsPortOccupied(frontendPort))
            {
                logger.LogInformation("Frontend React disponivel em {FrontendUrl}.", frontendUrl);
                OpenBrowser(logger, frontendUrl);
                return;
            }

            await Task.Delay(250);
        }

        logger.LogWarning("O frontend nao ficou disponivel em {FrontendUrl}. Verifique os logs do Vite.", frontendUrl);
    }

    private static async Task ForwardOutputAsync(Process process, ILogger logger)
    {
        var outputTask = PumpStreamAsync(process.StandardOutput, logger.LogInformation);
        var errorTask = PumpStreamAsync(process.StandardError, logger.LogWarning);

        await Task.WhenAll(outputTask, errorTask, process.WaitForExitAsync());

        if (process.ExitCode != 0)
        {
            logger.LogWarning("Processo do frontend finalizou com codigo {ExitCode}.", process.ExitCode);
        }
    }

    private static async Task PumpStreamAsync(StreamReader reader, Action<string, object?[]> logAction)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            logAction("Frontend: {Message}", [line]);
        }
    }

    private static void OpenBrowser(ILogger logger, string frontendUrl)
    {
        lock (BrowserLock)
        {
            if (_browserOpenAttempted)
            {
                return;
            }

            _browserOpenAttempted = true;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = frontendUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Nao foi possivel abrir o navegador em {FrontendUrl}.", frontendUrl);
        }
    }

    private static int ResolveFrontendPort(string portFilePath)
    {
        if (File.Exists(portFilePath))
        {
            File.Delete(portFilePath);
        }

        return PreferredFrontendPort;
    }

    private static bool IsPortOccupied(int port) =>
        IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpListeners()
            .Any(endpoint => endpoint.Port == port);
}



