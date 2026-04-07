# Anjian.FileArchiveWatcher

用于监听固定目录中的固定文件名。当外部系统反复导出并覆盖同名文件时，本工具会在检测到文件内容变化后，自动复制到归档目录，并按数字递增命名。

## 适用场景

- 外部系统每次都导出成同一个文件名
- 原文件会被新导出内容覆盖
- 需要自动保留每次导出的历史版本

## 功能说明

- 监听一个目录下的一个目标文件
- 检测 `Created`、`Changed`、`Renamed` 事件
- 自动等待文件写入完成，避免复制半成品
- 支持三种去重模式
- 归档文件名按数字递增并保留原扩展名，例如 `1.pdf`、`2.pdf`

## 命令行参数

```powershell
Anjian.FileArchiveWatcher --watch-dir <目录> --file-name <文件名> --output-dir <目录> [--dedupe md5|always|timestamp] [--poll-ms 500] [--stable-retries 3] [--debounce-ms 800]
```

参数说明：

- `--watch-dir`
  监听目录
- `--file-name`
  要监听的固定文件名，例如 `导出.pdf`
- `--output-dir`
  归档输出目录
- `--dedupe`
  去重策略，可选值：
  - `md5`：默认，仅当文件内容变化时归档
  - `always`：每次有效变化都归档
  - `timestamp`：按文件最后写入时间判断是否归档
- `--poll-ms`
  文件稳定性检测间隔，默认 `500`
- `--stable-retries`
  连续稳定次数，默认 `3`
- `--debounce-ms`
  事件防抖时间，默认 `800`

## 示例

默认使用 `md5` 去重：

```powershell
dotnet run --project D:\work\anjian\Anjian.FileArchiveWatcher\Anjian.FileArchiveWatcher.csproj -- --watch-dir D:\Exports --file-name 导出.pdf --output-dir D:\Exports\Archive
```

每次变化都保留一份：

```powershell
dotnet run --project D:\work\anjian\Anjian.FileArchiveWatcher\Anjian.FileArchiveWatcher.csproj -- --watch-dir D:\Exports --file-name 导出.pdf --output-dir D:\Exports\Archive --dedupe always
```

按文件时间去重：

```powershell
dotnet run --project D:\work\anjian\Anjian.FileArchiveWatcher\Anjian.FileArchiveWatcher.csproj -- --watch-dir D:\Exports --file-name 导出.pdf --output-dir D:\Exports\Archive --dedupe timestamp
```

## 运行方式

- 程序启动后会持续监听
- 控制台关闭后监听停止
- 按 `Ctrl + C` 可以安全退出

## 输出规则

- 归档目录已有 `1.pdf`、`2.pdf` 时，新文件会从 `3.pdf` 开始
- 默认只查找相同扩展名的数字文件作为编号基准
- 如果目标文件在程序启动时已经存在，程序会先检查一次并按规则决定是否归档

## 常见建议

- 建议把归档目录设置为和导出目录不同的位置，避免互相影响
- 默认优先用 `md5`，最适合“文件名总重复但只想保留真正新内容”的场景
- 如果你希望每次导出都保留，不管内容是否一样，使用 `always`
