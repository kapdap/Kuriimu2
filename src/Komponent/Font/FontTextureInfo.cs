using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Komponent.Font
{
    public class FontTextureInfo
    {
        public Image<Rgba32> FontTexture { get; }

        public IList<(Image<Rgba32>, Point)> Glyphs { get; }

        public FontTextureInfo(Image<Rgba32> fontTexture, IList<(Image<Rgba32>, Point)> glyphs)
        {
            FontTexture = fontTexture;
            Glyphs = glyphs;
        }
    }
}
