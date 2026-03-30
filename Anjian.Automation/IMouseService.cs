namespace Anjian;

public interface IMouseService
{
    void MoveTo(int x, int y);
    void LeftClick(int x, int y);
    void RightClick(int x, int y);
    void LeftDoubleClick(int x, int y, int intervalMilliseconds = 80);
    void LeftDown(int x, int y);
    void LeftUp(int x, int y);
}
