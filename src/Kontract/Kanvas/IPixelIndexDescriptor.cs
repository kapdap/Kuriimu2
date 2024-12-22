using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Kontract.Kanvas
{
    public interface IPixelIndexDescriptor
    {
        string GetPixelName();

        int GetBitDepth();

        Rgba32 GetColor(long value, IList<Rgba32> palette);

        long GetValue(int index, IList<Rgba32> palette);
    }
}
