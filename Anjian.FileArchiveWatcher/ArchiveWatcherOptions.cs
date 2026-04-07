using System;
using System.IO;

namespace Anjian;

public sealed record ArchiveWatcherOptions
{
    public required string WatchDirectory { get; init; }

    public required string TargetFileName { get; init; }

    public required string OutputDirectory { get; init; }

    public DedupeMode DedupeMode { get; init; } = DedupeMode.Md5;

    public int StabilityCheckIntervalMs { get; init; } = 500;

    public int StableReadRetryCount { get; init; } = 3;

    public int DebounceMilliseconds { get; init; } = 800;

    public string SourcePath => Path.Combine(WatchDirectory, TargetFileName);

    public ArchiveWatcherOptions Normalize()
    {
        if (string.IsNullOrWhiteSpace(WatchDirectory))
        {
            throw new ArgumentException("Watch directory cannot be empty.", nameof(WatchDirectory));
        }

        if (string.IsNullOrWhiteSpace(TargetFileName))
        {
            throw new ArgumentException("Target file name cannot be empty.", nameof(TargetFileName));
        }

        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            throw new ArgumentException("Output directory cannot be empty.", nameof(OutputDirectory));
        }

        if (StabilityCheckIntervalMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(StabilityCheckIntervalMs));
        }

        if (StableReadRetryCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(StableReadRetryCount));
        }

        if (DebounceMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(DebounceMilliseconds));
        }

        return this with
        {
            WatchDirectory = Path.GetFullPath(WatchDirectory),
            OutputDirectory = Path.GetFullPath(OutputDirectory),
            TargetFileName = Path.GetFileName(TargetFileName.Trim()),
        };
    }
}
