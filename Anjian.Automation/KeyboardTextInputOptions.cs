namespace Anjian;

public sealed record KeyboardTextInputOptions(
    KeyboardTextInputMode Mode = KeyboardTextInputMode.SendInput,
    bool RestoreClipboard = false,
    KeyboardPasteMode PasteMode = KeyboardPasteMode.CtrlV,
    int ClipboardSettleDelayMs = 80,
    int PasteSettleDelayMs = 80,
    int PerCharacterDelayMs = 50);
