using System.Collections.Generic;
using Kontract.Kanvas.Quantization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kontract.Kanvas.Configuration
{
    public delegate IColorCache CreateColorCacheDelegate(IList<Rgba32> palette);
    public delegate IList<Rgba32> CreatePaletteDelegate();
    public delegate IColorQuantizer CreateColorQuantizerDelegate(int colorCount, int taskCount);
    public delegate IColorDitherer CreateColorDithererDelegate(Size imageSize, int taskCount);

    public interface IQuantizationOptions
    {
        IQuantizationOptions WithColorCount(int colorCount);

        IQuantizationOptions WithColorCache(CreateColorCacheDelegate func);

        IQuantizationOptions WithPalette(CreatePaletteDelegate func);

        IQuantizationOptions WithColorQuantizer(CreateColorQuantizerDelegate func);

        IQuantizationOptions WithColorDitherer(CreateColorDithererDelegate func);
    }
}
