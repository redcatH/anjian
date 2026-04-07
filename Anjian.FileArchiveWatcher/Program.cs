using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Anjian;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length == 0 || HasFlag(args, "--help") || HasFlag(args, "-h"))
        {
            ShowUsage();
            return args.Length == 0 ? 1 : 0;
        }

        try
        {
            var options = ParseArguments(args);
            if (!Directory.Exists(options.WatchDirectory))
            {
                Console.WriteLine($"监听目录不存在: {options.WatchDirectory}");
                return 1;
            }

            using var watcher = new FileArchiveWatcherService(options);
            using var shutdown = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Console.WriteLine("正在停止监听...");
                shutdown.Set();
            };

            watcher.Start();
            Console.WriteLine("监听中，按 Ctrl+C 退出。");
            shutdown.Wait();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.GetBaseException().Message);
            Console.WriteLine();
            ShowUsage();
            return 1;
        }
    }

    private static ArchiveWatcherOptions ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var key = args[i];
            if (!key.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"无法识别的参数: {key}");
            }

            if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"参数缺少值: {key}");
            }

            values[key] = args[++i];
        }

        if (!values.TryGetValue("--watch-dir", out var watchDirectory))
        {
            throw new ArgumentException("缺少参数 --watch-dir");
        }

        if (!values.TryGetValue("--file-name", out var fileName))
        {
            throw new ArgumentException("缺少参数 --file-name");
        }

        if (!values.TryGetValue("--output-dir", out var outputDirectory))
        {
            throw new ArgumentException("缺少参数 --output-dir");
        }

        var dedupe = values.TryGetValue("--dedupe", out var dedupeValue)
            ? ParseDedupeMode(dedupeValue)
            : DedupeMode.Md5;

        var pollMs = values.TryGetValue("--poll-ms", out var pollValue)
            ? ParsePositiveInt("--poll-ms", pollValue)
            : 500;

        var stableRetries = values.TryGetValue("--stable-retries", out var retryValue)
            ? ParsePositiveInt("--stable-retries", retryValue)
            : 3;

        var debounceMs = values.TryGetValue("--debounce-ms", out var debounceValue)
            ? ParsePositiveInt("--debounce-ms", debounceValue)
            : 800;

        return new ArchiveWatcherOptions
        {
            WatchDirectory = watchDirectory,
            TargetFileName = fileName,
            OutputDirectory = outputDirectory,
            DedupeMode = dedupe,
            StabilityCheckIntervalMs = pollMs,
            StableReadRetryCount = stableRetries,
            DebounceMilliseconds = debounceMs,
        };
    }

    private static DedupeMode ParseDedupeMode(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "md5" => DedupeMode.Md5,
            "always" => DedupeMode.Always,
            "timestamp" => DedupeMode.Timestamp,
            _ => throw new ArgumentException($"不支持的去重模式: {value}"),
        };
    }

    private static int ParsePositiveInt(string name, string value)
    {
        if (!int.TryParse(value, out var parsed) || parsed <= 0)
        {
            throw new ArgumentException($"{name} 必须是大于 0 的整数。");
        }

        return parsed;
    }

    private static bool HasFlag(IEnumerable<string> args, string flag)
    {
        foreach (var arg in args)
        {
            if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void ShowUsage()
    {
        Console.WriteLine("用法:");
        Console.WriteLine("  Anjian.FileArchiveWatcher --watch-dir <目录> --file-name <文件名> --output-dir <目录> [--dedupe md5|always|timestamp] [--poll-ms 500] [--stable-retries 3] [--debounce-ms 800]");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine(@"  Anjian.FileArchiveWatcher --watch-dir D:\Exports --file-name 导出.pdf --output-dir D:\Exports\Archive");
        Console.WriteLine(@"  Anjian.FileArchiveWatcher --watch-dir D:\Exports --file-name 导出.pdf --output-dir D:\Exports\Archive --dedupe always");
        Console.WriteLine();
        Console.WriteLine("说明:");
        Console.WriteLine("  默认去重模式为 md5，仅当文件内容变化时才归档。");
        Console.WriteLine("  归档文件名按数字递增并保留原扩展名，例如 1.pdf、2.pdf。");
        Console.WriteLine("  程序启动后常驻监听，按 Ctrl+C 退出。");
    }
}
