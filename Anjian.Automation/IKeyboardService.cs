using System.Windows.Forms;

namespace Anjian;

public interface IKeyboardService
{
    void TextInput(string text);
    void TextInput(string text, KeyboardTextInputOptions options);
    void PasteText(string text, KeyboardPasteOptions? options = null);
    void KeyPress(Keys key);
    void KeyDown(Keys key);
    void KeyUp(Keys key);
    void HotKey(params Keys[] keys);
}
