using System.Diagnostics;

namespace SPI.Api.Extensions;

internal static class DevelopmentPortProcessCleaner
{
    private const int MaxReleaseAttempts = 5;
    private static readonly TimeSpan ReleaseWaitTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(400);

    public static void StopProcessesUsingPorts(IEnumerable<int> ports, Action<string>? log = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            log?.Invoke("Liberacao automatica de portas esta implementada apenas para Windows.");
            return;
        }

        foreach (var port in ports.Where(x => x > 0).Distinct().OrderBy(x => x))
        {
            StopProcessesUsingPort(port, log);
        }
    }

    private static void StopProcessesUsingPort(int port, Action<string>? log)
    {
        for (var attempt = 1; attempt <= MaxReleaseAttempts; attempt++)
        {
            var processIds = FindListeningProcessIds(port)
                .Where(processId => processId != Environment.ProcessId)
                .ToArray();

            if (processIds.Length == 0)
            {
                return;
            }

            foreach (var processId in processIds)
            {
                try
                {
                    using var process = Process.GetProcessById(processId);
                    var processName = process.ProcessName;
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit((int)ReleaseWaitTimeout.TotalMilliseconds);
                    log?.Invoke($"Processo '{processName}' (PID {processId}) encerrado para liberar a porta {port}.");
                }
                catch (ArgumentException)
                {
                    // The process finished between netstat and kill.
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Nao foi possivel encerrar o processo PID {processId} na porta {port}: {ex.Message}");
                }
            }

            if (WaitUntilReleased(port, ReleaseWaitTimeout))
            {
                return;
            }

            if (attempt < MaxReleaseAttempts)
            {
                Thread.Sleep(RetryDelay);
            }
        }

        var remainingProcessIds = string.Join(", ", FindListeningProcessIds(port).Where(processId => processId != Environment.ProcessId));
        log?.Invoke(string.IsNullOrWhiteSpace(remainingProcessIds)
            ? $"A porta {port} ainda parece ocupada, mas nenhum processo LISTENING foi identificado pelo netstat."
            : $"A porta {port} ainda esta ocupada apos tentativas de liberacao. PIDs restantes: {remainingProcessIds}.");
    }

    private static bool WaitUntilReleased(int port, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            var remainingProcessIds = FindListeningProcessIds(port)
                .Where(processId => processId != Environment.ProcessId)
                .ToArray();

            if (remainingProcessIds.Length == 0)
            {
                return true;
            }

            Thread.Sleep(200);
        }

        return false;
    }

    private static IReadOnlyCollection<int> FindListeningProcessIds(int port)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "netstat.exe",
            Arguments = "-ano -p tcp",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        });

        if (process is null)
        {
            return [];
        }

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(5000);

        var processIds = new HashSet<int>();
        foreach (var line in output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5 || !string.Equals(parts[0], "TCP", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var localAddress = parts[1];
            var state = parts[3];
            var processIdValue = parts[^1];

            if (!string.Equals(state, "LISTENING", StringComparison.OrdinalIgnoreCase) ||
                !EndpointUsesPort(localAddress, port) ||
                !int.TryParse(processIdValue, out var processId))
            {
                continue;
            }

            processIds.Add(processId);
        }

        return processIds;
    }

    private static bool EndpointUsesPort(string endpoint, int port) =>
        endpoint.EndsWith($":{port}", StringComparison.OrdinalIgnoreCase);
}
