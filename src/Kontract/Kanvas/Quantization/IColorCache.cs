using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kontract.Kanvas.Quantization
{
    /// <summary>
    /// Describes methods to cache and manage a limited amount colors.
    /// </summary>
    public interface IColorCache
    {
        /// <summary>
        /// The cached palette.
        /// </summary>
        IList<Rgba32> Palette { get; }

        /// <summary>
        /// Gets the index of the nearest color in the cache.
        /// </summary>
        /// <param name="color">The color to compare with.</param>
        /// <returns>Index of nearest color in the cache.</returns>
        int GetPaletteIndex(Rgba32 color);
    }
}
