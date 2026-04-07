using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Anjian;

public sealed class FileArchiveWatcherService : IDisposable
{
    private readonly ArchiveWatcherOptions _options;
    private readonly SemaphoreSlim _processingLock = new(1, 1);
    private readonly object _stateLock = new();
    private FileSystemWatcher? _watcher;
    private Timer? _debounceTimer;
    private int _nextSequenceNumber = 1;
    private string? _lastArchivedMd5;
    private DateTime? _lastObservedSourceWriteTimeUtc;
    private string _pendingReason = "startup";
    private bool _disposed;

    public FileArchiveWatcherService(ArchiveWatcherOptions options)
    {
        _options = options.Normalize();
    }

    public void Start()
    {
        ThrowIfDisposed();

        Directory.CreateDirectory(_options.WatchDirectory);
        Directory.CreateDirectory(_options.OutputDirectory);
        InitializeSequenceState();

        _watcher = new FileSystemWatcher(_options.WatchDirectory, _options.TargetFileName)
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
            EnableRaisingEvents = true,
        };

        _watcher.Created += OnFileEvent;
        _watcher.Changed += OnFileEvent;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnWatcherError;

        Log($"开始监听: {_options.SourcePath}");
        Log($"归档目录: {_options.OutputDirectory}");
        Log($"去重模式: {_options.DedupeMode}");

        if (File.Exists(_options.SourcePath))
        {
            ScheduleProcessing("startup-existing-file");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _debounceTimer?.Dispose();
        _watcher?.Dispose();
        _processingLock.Dispose();
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        if (!IsTargetFile(e.FullPath))
        {
            return;
        }

        ScheduleProcessing(e.ChangeType.ToString());
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        if (!IsTargetFile(e.FullPath) && !IsTargetFile(e.OldFullPath))
        {
            return;
        }

        ScheduleProcessing("Renamed");
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        Log($"监听器发生错误: {e.GetException().GetBaseException().Message}");
        ScheduleProcessing("WatcherErrorRecovery");
    }

    private void ScheduleProcessing(string reason)
    {
        lock (_stateLock)
        {
            _pendingReason = reason;
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(
                static state => ((FileArchiveWatcherService)state!).OnDebounceElapsed(),
                this,
                _options.DebounceMilliseconds,
                Timeout.Infinite);
        }
    }

    private void OnDebounceElapsed()
    {
        _ = ProcessPendingAsync();
    }

    private async Task ProcessPendingAsync()
    {
        await _processingLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!File.Exists(_options.SourcePath))
            {
                Log("检测到事件，但目标文件当前不存在，已跳过。");
                return;
            }

            var snapshot = await WaitForStableSnapshotAsync().ConfigureAwait(false);
            if (snapshot is null)
            {
                Log("目标文件长时间未稳定，已跳过本次归档。");
                return;
            }

            if (ShouldSkip(snapshot))
            {
                Log($"检测到文件变化，但根据 {_options.DedupeMode} 规则无需归档。");
                return;
            }

            var archived = Archive(snapshot);
            Log($"已归档: {archived.ArchivedPath}");
        }
        catch (Exception ex)
        {
            Log($"归档失败: {ex.GetBaseException().Message}");
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private bool ShouldSkip(FileSnapshot snapshot)
    {
        return _options.DedupeMode switch
        {
            DedupeMode.Always => false,
            DedupeMode.Md5 => string.Equals(_lastArchivedMd5, snapshot.Md5, StringComparison.OrdinalIgnoreCase),
            DedupeMode.Timestamp => _lastObservedSourceWriteTimeUtc == snapshot.LastWriteTimeUtc,
            _ => false,
        };
    }

    private ArchivedFileRecord Archive(FileSnapshot snapshot)
    {
        var archivedPath = GetNextArchivePath();
        File.Copy(_options.SourcePath, archivedPath, overwrite: false);

        _lastArchivedMd5 = snapshot.Md5;
        _lastObservedSourceWriteTimeUtc = snapshot.LastWriteTimeUtc;

        return new ArchivedFileRecord(
            ExtractSequenceNumber(archivedPath),
            _options.SourcePath,
            archivedPath,
            snapshot.Md5,
            DateTime.Now,
            $"Archived from {_pendingReason}");
    }

    private void InitializeSequenceState()
    {
        var extension = Path.GetExtension(_options.TargetFileName);
        var files = Directory.EnumerateFiles(_options.OutputDirectory, $"*{extension}", SearchOption.TopDirectoryOnly)
            .Select(static path => new
            {
                Path = path,
                Sequence = ExtractSequenceNumber(path),
            })
            .Where(static item => item.Sequence > 0)
            .OrderByDescending(static item => item.Sequence)
            .ToArray();

        if (files.Length == 0)
        {
            _nextSequenceNumber = 1;
            return;
        }

        _nextSequenceNumber = files[0].Sequence + 1;

        if (_options.DedupeMode == DedupeMode.Md5)
        {
            _lastArchivedMd5 = ComputeMd5(files[0].Path);
        }
    }

    private string GetNextArchivePath()
    {
        lock (_stateLock)
        {
            var extension = Path.GetExtension(_options.TargetFileName);
            while (true)
            {
                var candidate = Path.Combine(_options.OutputDirectory, $"{_nextSequenceNumber}{extension}");
                _nextSequenceNumber++;
                if (!File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }
    }

    private async Task<FileSnapshot?> WaitForStableSnapshotAsync()
    {
        FileSnapshot? previous = null;
        var stableCount = 0;

        for (var attempt = 0; attempt < 60; attempt++)
        {
            try
            {
                var info = new FileInfo(_options.SourcePath);
                info.Refresh();
                if (!info.Exists)
                {
                    return null;
                }

                using var stream = new FileStream(
                    _options.SourcePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                var md5 = ComputeMd5(stream);
                var snapshot = new FileSnapshot(info.Length, info.LastWriteTimeUtc, md5);

                if (previous is not null &&
                    previous.Length == snapshot.Length &&
                    previous.LastWriteTimeUtc == snapshot.LastWriteTimeUtc &&
                    string.Equals(previous.Md5, snapshot.Md5, StringComparison.OrdinalIgnoreCase))
                {
                    stableCount++;
                    if (stableCount >= _options.StableReadRetryCount)
                    {
                        return snapshot;
                    }
                }
                else
                {
                    stableCount = 1;
                    previous = snapshot;
                }
            }
            catch (IOException)
            {
                stableCount = 0;
                previous = null;
            }
            catch (UnauthorizedAccessException)
            {
                stableCount = 0;
                previous = null;
            }

            await Task.Delay(_options.StabilityCheckIntervalMs).ConfigureAwait(false);
        }

        return null;
    }

    private bool IsTargetFile(string fullPath)
    {
        return string.Equals(
            Path.GetFullPath(fullPath),
            Path.GetFullPath(_options.SourcePath),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeMd5(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return ComputeMd5(stream);
    }

    private static string ComputeMd5(Stream stream)
    {
        stream.Position = 0;
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    private static int ExtractSequenceNumber(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        return int.TryParse(fileName, out var value) ? value : -1;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    private sealed record FileSnapshot(long Length, DateTime LastWriteTimeUtc, string Md5);
}
