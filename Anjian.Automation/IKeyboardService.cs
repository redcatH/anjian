using System.Windows.Forms;

namespace Anjian;

public interface IKeyboardService
{
    void TextInput(string text);
    void KeyPress(Keys key);
    void KeyDown(Keys key);
    void KeyUp(Keys key);
    void HotKey(params Keys[] keys);
}
