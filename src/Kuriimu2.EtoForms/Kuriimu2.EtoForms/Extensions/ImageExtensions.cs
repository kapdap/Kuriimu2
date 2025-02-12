using System.IO;
using Eto.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kuriimu2.EtoForms.Extensions
{
    public static class ImageExtensions
    {
        public static Bitmap ToEto(this Image<Rgba32> image)
        {
            // HINT: Substitute solution; Convert to PNG and load it with Eto
            var ms = new MemoryStream();
            image.SaveAsPng(ms);

            ms.Position = 0;
            return new Bitmap(ms);
        }
    }
}
