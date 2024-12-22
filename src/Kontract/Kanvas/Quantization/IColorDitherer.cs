using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Kontract.Kanvas.Quantization
{
    /// <summary>
    /// Describes methods to quantize and dither a collection of colors.
    /// </summary>
    public interface IColorDitherer
    {
        /// <summary>
        /// Quantizes and dithers a collection of colors.
        /// </summary>
        /// <param name="colors">The collection to quantize and dither.</param>
        /// <param name="colorCache"></param>
        /// <returns></returns>
        IEnumerable<int> Process(IEnumerable<Rgba32> colors, IColorCache colorCache);
    }
}
