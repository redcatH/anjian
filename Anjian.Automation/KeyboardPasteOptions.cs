namespace Anjian;

public sealed record KeyboardPasteOptions(
    bool RestoreClipboard = false,
    KeyboardPasteMode PasteMode = KeyboardPasteMode.CtrlV,
    int ClipboardSettleDelayMs = 80,
    int PasteSettleDelayMs = 80);
