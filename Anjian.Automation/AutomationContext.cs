using System;

namespace Anjian;

public sealed class AutomationContext
{
    public AutomationContext(
        IMouseService mouse,
        IKeyboardService keyboard,
        IImageMatcher matcher,
        IOcrService ocr,
        ScreenCaptureService capture,
        ImagePreprocessService imagePreprocess,
        IExecutionController executionController)
    {
        Mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
        Keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
        Matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
        Ocr = ocr ?? throw new ArgumentNullException(nameof(ocr));
        Capture = capture ?? throw new ArgumentNullException(nameof(capture));
        ImagePreprocess = imagePreprocess ?? throw new ArgumentNullException(nameof(imagePreprocess));
        ExecutionController = executionController ?? throw new ArgumentNullException(nameof(executionController));
    }

    public IMouseService Mouse { get; }

    public IKeyboardService Keyboard { get; }

    public IImageMatcher Matcher { get; }

    public IOcrService Ocr { get; }

    public ScreenCaptureService Capture { get; }

    public ImagePreprocessService ImagePreprocess { get; }

    public IExecutionController ExecutionController { get; }
}
