using System.Diagnostics;

namespace SPI.Api.Extensions;

internal static class DevelopmentPortProcessCleaner
{
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
        foreach (var processId in FindListeningProcessIds(port))
        {
            if (processId == Environment.ProcessId)
            {
                continue;
            }

            try
            {
                using var process = Process.GetProcessById(processId);
                var processName = process.ProcessName;
                process.Kill(entireProcessTree: true);
                process.WaitForExit(3000);
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
