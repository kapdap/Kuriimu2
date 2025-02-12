using System.Collections.Generic;
using Kontract;
using Kontract.Kanvas.Quantization;
using SixLabors.ImageSharp.PixelFormats;

namespace Kanvas.Quantization.ColorCaches
{
    public abstract class BaseColorCache : IColorCache
    {
        /// <inheritdoc />
        public IList<Rgba32> Palette { get; }

        public BaseColorCache(IList<Rgba32> palette)
        {
            ContractAssertions.IsNotNull(palette,nameof(palette));

            Palette = palette;
        }

        /// <inheritdoc />
        public abstract int GetPaletteIndex(Rgba32 color);
    }
}
