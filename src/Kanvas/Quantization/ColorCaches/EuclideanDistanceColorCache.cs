using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Kontract.Kanvas.Quantization;
using SixLabors.ImageSharp.PixelFormats;

namespace Kanvas.Quantization.ColorCaches
{
    /// <summary>
    /// The <see cref="IColorCache"/> to search colors with euclidean distance.
    /// </summary>
    public class EuclideanDistanceColorCache : BaseColorCache
    {
        private readonly ConcurrentDictionary<Rgba32, int> _cache;

        public EuclideanDistanceColorCache(IList<Rgba32> palette) :
            base(palette)
        {
            _cache = new ConcurrentDictionary<Rgba32, int>();
        }

        /// <inheritdoc />
        public override int GetPaletteIndex(Rgba32 color)
        {
            return _cache.AddOrUpdate(color,
                colorKey =>
                {
                    int paletteIndexInside = CalculatePaletteIndexInternal(color);
                    return paletteIndexInside;
                },
                (colorKey, inputIndex) => inputIndex);
        }

        private int CalculatePaletteIndexInternal(Rgba32 color)
        {
            return EuclideanHelper.GetSmallestEuclideanDistanceIndex(Palette, color);
        }
    }
}
