using System;
using Kontract.Interfaces.Progress;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace Kontract.Kanvas
{
    public interface IImageTranscoder
    {
        /// <summary>
        /// Decodes the image data to an image.
        /// </summary>
        /// <param name="imageData">The image data to decode.</param>
        /// <param name="imageSize">The size of the decoded image.</param>
        /// <param name="progress">The progress for this action.</param>
        /// <returns>The decoded image.</returns>
        /// <exception cref="ArgumentNullException">If the data to decode is expected to retrieve palette data.</exception>
        Image<Rgba32> Decode(byte[] imageData, Size imageSize, IProgressContext progress = null);

        /// <summary>
        /// Decodes the image and palette data to an image.
        /// </summary>
        /// <param name="imageData">The image data to decode.</param>
        /// <param name="paletteData">The palette data to decode.</param>
        /// <param name="imageSize">The size of the decoded image.</param>
        /// <param name="progress">The progress for this action.</param>
        /// <returns>The decoded image.</returns>
        Image<Rgba32> Decode(byte[] imageData, byte[] paletteData, Size imageSize, IProgressContext progress = null);

        /// <summary>
        /// Encodes the image to its image data.
        /// </summary>
        /// <param name="image">The image to encode.</param>
        /// <param name="progress">The progress for this action.</param>
        /// <returns>The encoded image and palette data.</returns>
        /// <remarks>Palette data is <see langword="null" />, if the transcoder is not setup for indexed images.</remarks>
        (byte[] imageData, byte[] paletteData) Encode(Image<Rgba32> image, IProgressContext progress = null);
    }
}
