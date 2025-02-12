using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Kanvas.Quantization.Models.ColorCache
{
    class LshBucketInfo
    {
        public SortedDictionary<int, Rgba32> Colors;

        /// <summary>
        /// Initializes a new instance of the <see cref="LshBucketInfo"/> class.
        /// </summary>
        public LshBucketInfo()
        {
            Colors = new SortedDictionary<int, Rgba32>();
        }

        /// <summary>
        /// Adds the color to the bucket information.
        /// </summary>
        /// <param name="paletteIndex">Index of the palette.</param>
        /// <param name="color">The color.</param>
        public void AddColor(int paletteIndex, Rgba32 color)
        {
            Colors.Add(paletteIndex, color);
        }
    }
}
