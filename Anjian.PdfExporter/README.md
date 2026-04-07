# Anjian.PdfExporter

用于批量读取指定目录下的 PDF，按文件最后修改时间升序排序，提取每个 PDF 中的：

- 人员姓名
- `照护费合计` 行的 `全额统筹` 列

最后将结果导出为 CSV。

## 适用场景

- 一批 PDF 都是同一类结算清单
- 需要按时间顺序整理导出结果
- 需要快速生成一个可在 Excel 中查看的 CSV

## 提取规则

- 只读取每个 PDF 的第一页
- 姓名使用首页字段提取规则
- 金额使用表格交叉取值规则：
  - 行名：`照护费合计`
  - 列名：`全额统筹`

## 命令行参数

```powershell
Anjian.PdfExporter <pdf目录> [输出csv路径]
```

参数说明：

- `pdf目录`
  需要批量处理的 PDF 文件夹
- `输出csv路径`
  可选。不传时会默认输出到输入目录下，文件名格式为 `pdf-export-时间戳.csv`

## 示例

输出到默认 CSV：

```powershell
dotnet run --project D:\work\anjian\Anjian.PdfExporter\Anjian.PdfExporter.csproj -- D:\pdfs
```

指定输出路径：

```powershell
dotnet run --project D:\work\anjian\Anjian.PdfExporter\Anjian.PdfExporter.csproj -- D:\pdfs D:\output\result.csv
```

## CSV 列说明

生成的 CSV 包含以下列：

- 文件时间
- 文件名
- 姓名
- 全额统筹
- 是否成功
- 消息
- 文件路径

## 排序规则

- 按 PDF 文件最后修改时间升序排列
- 如果时间相同，则按文件名排序

## 使用建议

- 先确保这一批 PDF 都是同一模板或接近模板
- 如果某些文件提取失败，可以先用 `Anjian.Tool` 里的 PDF 验证工具检查单张结果
- CSV 使用 UTF-8 BOM 输出，通常可直接用 Excel 打开

## 注意事项

- 当前版本默认只处理当前目录下的 PDF，不递归子目录
- 如果目录里没有 PDF，程序会直接提示并退出
- 如果 PDF 模板变化较大，可能需要调整提取规则后再批量导出
