using SixLabors.ImageSharp.PixelFormats;

namespace Kontract.Kanvas
{
    public interface IColorShader
    {
        Rgba32 Read(Rgba32 c);

        Rgba32 Write(Rgba32 c);
    }
}
