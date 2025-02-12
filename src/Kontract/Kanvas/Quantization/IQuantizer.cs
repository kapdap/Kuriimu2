using System.Collections.Generic;
using Kontract.Interfaces.Progress;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace Kontract.Kanvas.Quantization
{
    public interface IQuantizer
    {
        Image<Rgba32> ProcessImage(Image<Rgba32> image, IProgressContext progress = null);

        (IEnumerable<int>, IList<Rgba32>) Process(IEnumerable<Rgba32> colors, Size imageSize, IProgressContext progress = null);
    }
}
