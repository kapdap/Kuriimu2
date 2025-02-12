using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kanvas;
using Kanvas.Encoding;
using Komponent.IO.Attributes;
using Kontract.Kanvas;
using Kontract.Models.Image;
using SixLabors.ImageSharp.PixelFormats;

namespace plugin_sega.Images
{
    class HtexHeader
    {
        [FixedLength(4)]
        public string magic;
        public int sectionSize;
        public uint data1;
        public uint data2;
    }

    class HtexSupport
    {
        private static readonly IDictionary<int, IIndexEncoding> IndexFormats = new Dictionary<int, IIndexEncoding>
        {
            [0] = ImageFormats.I8()
        };

        private static readonly IDictionary<int, IColorEncoding> PaletteEncodings = new Dictionary<int, IColorEncoding>
        {
            [0x6C09] = new Rgba(8, 8, 8, 8, "ABGR"),
            [0x6409] = new Rgba(8, 8, 8, 8, "ABGR")
        };

        public static EncodingDefinition GetEncodingDefinition()
        {
            var definition = new EncodingDefinition();

            definition.AddPaletteEncodings(PaletteEncodings);
            definition.AddIndexEncodings(IndexFormats.Select(x => (x.Key, new IndexEncodingDefinition(x.Value, new[] { 0x6C09, 0x6409 }))).ToArray());

            definition.AddPaletteShader(0x6C09, new HtexColorShader());

            return definition;
        }
    }

    class HtexColorShader : IColorShader
    {
        public Rgba32 Read(Rgba32 c)
        {
            return new Rgba32(c.R, c.G, c.B, c.A * 0xFF / 0x80);
        }

        public Rgba32 Write(Rgba32 c)
        {
            return new Rgba32(c.R, c.G, c.B, c.A * 0x80 / 0xFF);
        }
    }
}
