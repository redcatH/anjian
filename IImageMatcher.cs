using System.Drawing;

namespace Anjian;

public interface IImageMatcher
{
    ImageMatchResult Find(Bitmap source, Bitmap template, ImageMatchOptions options);
}
