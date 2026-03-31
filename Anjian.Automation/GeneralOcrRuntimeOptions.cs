namespace Anjian;

public sealed record GeneralOcrRuntimeOptions(
    OcrEngineType Provider = OcrEngineType.PaddleSharp,
    GeneralOcrDeviceType Device = GeneralOcrDeviceType.CpuMkl);
