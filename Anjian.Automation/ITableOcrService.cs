using System.Drawing;
using System.Threading.Tasks;

namespace Anjian;

public interface ITableOcrService
{
    Task<TableOcrResult> RecognizeTableAsJsonAsync(Bitmap image);
}
