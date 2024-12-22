using SixLabors.ImageSharp.PixelFormats;

namespace Kontract.Kanvas
{
    public interface IPixelDescriptor
    {
        string GetPixelName();

        int GetBitDepth();

        Rgba32 GetColor(long value);

        long GetValue(Rgba32 color);
    }
}
