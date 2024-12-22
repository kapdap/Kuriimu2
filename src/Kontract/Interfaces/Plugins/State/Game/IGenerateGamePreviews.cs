using Kontract.Models.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kontract.Interfaces.Plugins.State.Game
{
    /// <summary>
    /// This is the game adapter interface for creating game preview plugins.
    /// </summary>
    public interface IGenerateGamePreviews
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        Image<Rgba32> GeneratePreview(TextEntry entry);
    }
}
