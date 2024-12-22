using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace Komponent.Font
{
    /// <summary>
    /// Static methods for font measurements.
    /// </summary>
    public static class FontMeasurement
    {
        /// <summary>
        /// Measure the whitespace of glyphs.
        /// </summary>
        /// <param name="glyphs">The glyphs to measure.</param>
        /// <returns>The measured glyphs.</returns>
        public static IEnumerable<AdjustedGlyph> MeasureWhiteSpace(IEnumerable<Image<Rgba32>> glyphs)
        {
            foreach (var glyph in glyphs)
            {
                var top = MeasureWhiteSpaceSide(0, glyph);
                var left = MeasureWhiteSpaceSide(1, glyph);
                var bottom = MeasureWhiteSpaceSide(2, glyph);
                var right = MeasureWhiteSpaceSide(3, glyph);

                var adjustment = new WhiteSpaceAdjustment(
                    new Point(left, top),
                    new Size(glyph.Width - left - right, glyph.Height - top - bottom));
                yield return new AdjustedGlyph(glyph, adjustment);
            }
        }

        private static int MeasureWhiteSpaceSide(int mode, Image<Rgba32> glyph)
        {
            switch (mode)
            {
                // Top
                case 0:
                    for (var y = 0; y < glyph.Height; y++)
                        for (var x = 0; x < glyph.Width; x++)
                            if ((Color)glyph[x, y] != Color.Transparent)
                                return y;

                    return glyph.Height;

                // Left
                case 1:
                    for (var x = 0; x < glyph.Width; x++)
                        for (var y = 0; y < glyph.Height; y++)
                            if ((Color)glyph[x, y] != Color.Transparent)
                                return x;

                    return glyph.Width;

                // Bottom
                case 2:
                    for (int y = glyph.Height - 1; y >= 0; y--)
                        for (var x = 0; x < glyph.Width; x++)
                            if ((Color)glyph[x, y] != Color.Transparent)
                                return glyph.Height - y - 1;

                    return 0;

                // Right
                case 3:
                    for (var x = 0; x < glyph.Width; x++)
                        for (var y = 0; y < glyph.Height; y++)
                            if ((Color)glyph[x, y] != Color.Transparent)
                                return glyph.Width - x - 1;

                    return 0;

                default:
                    return -1;
            }
        }
    }
}
