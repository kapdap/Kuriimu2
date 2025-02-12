using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Kanvas.Quantization.Models.Ditherer
{
    class ErrorDiffusionElement
    {
        private readonly IList<Rgba32> _colors;
        private readonly int _colorIndex;

        public Rgba32 Color => _colors[_colorIndex];

        public IDictionary<int, ColorComponentError> Errors { get; }

        public IList<int> Indices { get; }

        public ErrorDiffusionElement(IList<Rgba32> colors, int colorIndex, IDictionary<int, ColorComponentError> errors, IList<int> indices)
        {
            _colors = colors;
            _colorIndex = colorIndex;

            Errors = errors;
            Indices = indices;
        }
    }
}
