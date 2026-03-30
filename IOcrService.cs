using System.Drawing;
using System.Threading.Tasks;

namespace Anjian;

public interface IOcrService
{
    Task<AmountOcrResult> RecognizeAmountAsync(Bitmap image);
}
