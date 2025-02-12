using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace Komponent.Extensions
{
    public static class ImageExtensions
    {
        public static void PutChannel(this Image<Rgba32> bitmap, Image<Rgba32> channel)
        {
            for (var y = 0; y < channel.Height && y < bitmap.Height; y++)
            {
                for (var x = 0; x < channel.Width && x < bitmap.Width; x++)
                {
                    Rgba32 color = bitmap[x, y];
                    Rgba32 channelColor = channel[x, y];

                    var red = (byte)(color.R | channelColor.R);
                    var green = (byte)(color.G | channelColor.G);
                    var blue = (byte)(color.B | channelColor.B);
                    var alpha = (byte)(color.A | channelColor.A);

                    bitmap[x, y] = new Rgba32(red, green, blue, alpha);
                }
            }
        }
    }
}
