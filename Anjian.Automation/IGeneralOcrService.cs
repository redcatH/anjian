using System.Drawing;
using System.Threading.Tasks;

namespace Anjian;

public interface IGeneralOcrService
{
    Task<GeneralOcrResult> RecognizeTextAsync(Bitmap image, GeneralOcrOptions? options = null);

    Task<GeneralOcrResult> RecognizeRegionsAsync(Bitmap image, GeneralOcrOptions? options = null);
}
