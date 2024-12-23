using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Komponent.Font;
using Komponent.IO;
using Kontract.Models.Font;
using plugin_level5.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace plugin_level5._3DS.Fonts
{
    public class Xf
    {
        private Level5CompressionMethod _t0Comp;
        private Level5CompressionMethod _t1Comp;
        private Level5CompressionMethod _t2Comp;

        private readonly ColorMatrix[] _colorMatrices =
        {
            new(
                0f, 0f, 0f, 1f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                1f, 1f, 1f, 0f),
            new(
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 1f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                1f, 1f, 1f, 0f),
            new(
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 1f,
                0f, 0f, 0f, 0f,
                1f, 1f, 1f, 0f),
        };

        private readonly ColorMatrix[] _inverseColorMatrices =
        {
            new(
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                1f, 0f, 0f, 0f,
                0f, 0f, 0f, 1f),
            new(
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 1f, 0f, 0f,
                0f, 0f, 0f, 1f),
            new(
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 1f, 0f,
                0f, 0f, 0f, 1f),
        };

        public XfHeader Header { get; private set; }

        public List<CharacterInfo> Load(Stream fntFile, Image fontImage)
        {
            using var br = new BinaryReaderX(fntFile);

            //var f = File.Create(@"D:\Users\Kirito\Desktop\reverse_engineering\time_travelers\font\test.bin");
            //br.BaseStream.CopyTo(f);
            //f.Close();
            //br.BaseStream.Position = 0;

            // Read header
            Header = br.ReadType<XfHeader>();

            // Read charSizeInfo
            br.BaseStream.Position = Header.charSizeOffset << 2;
            _t0Comp = Level5Compressor.PeekCompressionMethod(br.BaseStream);
            var charSizeStream = Decompress(br.BaseStream);
            var charSizeInfos = new BinaryReaderX(charSizeStream).ReadMultiple<XfCharSizeInfo>(Header.charSizeCount).ToArray();

            // Read large chars
            br.BaseStream.Position = Header.largeCharOffset << 2;
            _t1Comp = Level5Compressor.PeekCompressionMethod(br.BaseStream);
            var largeCharStream = Decompress(br.BaseStream);
            var largeChars = new BinaryReaderX(largeCharStream).ReadMultiple<XfCharMap>(Header.largeCharCount).ToArray();

            // Read small chars
            br.BaseStream.Position = Header.smallCharOffset << 2;
            _t2Comp = Level5Compressor.PeekCompressionMethod(br.BaseStream);
            var smallCharStream = Decompress(br.BaseStream);
            var smallChars = new BinaryReaderX(smallCharStream).ReadMultiple<XfCharMap>(Header.smallCharCount).ToArray();

            // Load characters (ignore small chars)
            var result = new List<CharacterInfo>(largeChars.Length);
            foreach (var largeChar in largeChars)
            {
                var charSizeInfo = charSizeInfos[largeChar.charInformation.charSizeInfoIndex];
                var glyph = GetGlyphBitmap(fontImage, largeChar, charSizeInfo);

                var characterInfo = new CharacterInfo(largeChar.codePoint, new Size(largeChar.charInformation.charWidth, 0), glyph);
                result.Add(characterInfo);
            }

            return result;
        }

        public (Stream fontStream, Image<Rgba32> fontImage) Save(List<CharacterInfo> characterInfos, Size imageSize)
        {
            // Generating font textures
            var adjustedGlyphs = FontMeasurement.MeasureWhiteSpace(characterInfos.Select(x => x.Glyph)).ToList();

            // Adjust image size for at least the biggest letter
            var height = Math.Max(adjustedGlyphs.Max(x => x.WhiteSpaceAdjustment.GlyphSize.Height), imageSize.Height);
            var width = Math.Max(adjustedGlyphs.Max(x => x.WhiteSpaceAdjustment.GlyphSize.Width), imageSize.Width);
            imageSize = new Size(width, height);

            var generator = new FontTextureGenerator(imageSize, 0);
            var textureInfos = generator.GenerateFontTextures(adjustedGlyphs, 3).ToList();

            // Join important lists
            var joinedCharacters = characterInfos.OrderBy(x => x.CodePoint).Join(adjustedGlyphs, c => c.Glyph, ag => ag.Glyph,
                (c, ag) => new { character = c, adjustedGlyph = ag })
                .Select(cag => new
                {
                    cag.character,
                    cag.adjustedGlyph,
                    textureIndex = textureInfos.FindIndex(x => x.Glyphs.Any(y => y.Item1 == cag.adjustedGlyph.Glyph)),
                    texturePosition = textureInfos.SelectMany(x => x.Glyphs).FirstOrDefault(x => x.Item1 == cag.adjustedGlyph.Glyph).Item2
                });

            // Create character information
            var charMaps = new List<(AdjustedGlyph, XfCharMap)>(adjustedGlyphs.Count);
            var charSizeInfos = new List<XfCharSizeInfo>();
            foreach (var joinedCharacter in joinedCharacters)
            {
                if (joinedCharacter.textureIndex == -1)
                    continue;

                var charSizeInfo = new XfCharSizeInfo
                {
                    offsetX = (sbyte)joinedCharacter.adjustedGlyph.WhiteSpaceAdjustment.GlyphPosition.X,
                    offsetY = (sbyte)joinedCharacter.adjustedGlyph.WhiteSpaceAdjustment.GlyphPosition.Y,
                    glyphWidth = (byte)joinedCharacter.adjustedGlyph.WhiteSpaceAdjustment.GlyphSize.Width,
                    glyphHeight = (byte)joinedCharacter.adjustedGlyph.WhiteSpaceAdjustment.GlyphSize.Height
                };
                if (!charSizeInfos.Contains(charSizeInfo))
                    charSizeInfos.Add(charSizeInfo);

                // Only used for Time Travelers
                var codePoint = ConvertChar(joinedCharacter.character.CodePoint);
                //var codePoint = joinedCharacter.character.CodePoint;

                var charInformation = new XfCharInformation
                {
                    charSizeInfoIndex = charSizeInfos.IndexOf(charSizeInfo),
                    charWidth = char.IsWhiteSpace((char)codePoint) ?
                        joinedCharacter.character.CharacterSize.Width :
                        joinedCharacter.character.CharacterSize.Width - charSizeInfo.offsetX
                };

                // Only used for Time Travelers
                charInformation.charWidth--;

                charMaps.Add((joinedCharacter.adjustedGlyph, new XfCharMap
                {
                    codePoint = (ushort)codePoint,
                    charInformation = charInformation,
                    imageInformation = new XfImageInformation
                    {
                        colorChannel = joinedCharacter.textureIndex,
                        imageOffsetX = joinedCharacter.texturePosition.X,
                        imageOffsetY = joinedCharacter.texturePosition.Y
                    }
                }));

                if (codePoint != joinedCharacter.character.CodePoint)
                {
                    charInformation = new XfCharInformation
                    {
                        charSizeInfoIndex = charSizeInfos.IndexOf(charSizeInfo),
                        charWidth = char.IsWhiteSpace((char)joinedCharacter.character.CodePoint)
                            ? joinedCharacter.character.CharacterSize.Width
                            : joinedCharacter.character.CharacterSize.Width - charSizeInfo.offsetX
                    };

                    charMaps.Add((joinedCharacter.adjustedGlyph, new XfCharMap
                    {
                        codePoint = (ushort)joinedCharacter.character.CodePoint,
                        charInformation = charInformation,
                        imageInformation = new XfImageInformation
                        {
                            colorChannel = joinedCharacter.textureIndex,
                            imageOffsetX = joinedCharacter.texturePosition.X,
                            imageOffsetY = joinedCharacter.texturePosition.Y
                        }
                    }));

                    // Only used for Time Travelers
                    charInformation.charWidth--;
                }
            }

            // Set escape characters
            var escapeIndex = charMaps.FindIndex(x => x.Item2.codePoint == '?');
            Header.largeEscapeCharacter = escapeIndex < 0 ? (short)0 : (short)escapeIndex;
            Header.smallEscapeCharacter = 0;

            // Minimize top value and line height
            Header.largeCharHeight = (short)charSizeInfos.Max(x => x.glyphHeight + x.offsetY);
            Header.smallCharHeight = 0;

            // Draw textures
            var img = new Image<Rgba32>(imageSize.Width, imageSize.Height);
            var chn = new Image<Rgba32>(imageSize.Width, imageSize.Height);
            for (var i = 0; i < textureInfos.Count; i++)
                img.Mutate(context => context.Filter(_inverseColorMatrices[i]).DrawImage(textureInfos[i].FontTexture, 1f));

            // Save fnt.bin
            var savedFntBin = new MemoryStream();
            using (var bw = new BinaryWriterX(savedFntBin, true))
            {
                //Table0
                bw.BaseStream.Position = 0x28;
                Header.charSizeCount = (short)charSizeInfos.Count;
                WriteMultipleCompressed(bw, charSizeInfos, _t0Comp);
                bw.WriteAlignment(4);

                //Table1
                Header.largeCharOffset = (short)(bw.BaseStream.Position >> 2);
                Header.largeCharCount = (short)charMaps.Count;
                WriteMultipleCompressed(bw, charMaps.OrderBy(x => x.Item2.codePoint).Select(x => x.Item2).ToArray(), _t1Comp);
                bw.WriteAlignment(4);

                //Table2
                Header.smallCharOffset = (short)(bw.BaseStream.Position >> 2);
                Header.smallCharCount = 0;
                WriteMultipleCompressed(bw, Array.Empty<XfCharMap>(), _t2Comp);
                bw.WriteAlignment(4);

                //Header
                bw.BaseStream.Position = 0;
                bw.WriteType(Header);
            }

            return (savedFntBin, img);
        }

        private Image<Rgba32> GetGlyphBitmap(Image fontImage, XfCharMap charMap, XfCharSizeInfo charSizeInfo)
        {
            // Destination points
            var destRect = new Rectangle(
                charSizeInfo.offsetX, charSizeInfo.offsetY,
                charSizeInfo.glyphWidth, charSizeInfo.glyphHeight);

            // Source rectangle
            var srcRect = new Rectangle(
                charMap.imageInformation.imageOffsetX,
                charMap.imageInformation.imageOffsetY,
                charSizeInfo.glyphWidth,
                charSizeInfo.glyphHeight);

            // Draw the glyph from the master texture
            var glyph = new Image<Rgba32>(
                Math.Max(1, Math.Max(charMap.charInformation.charWidth, charSizeInfo.glyphWidth + charSizeInfo.offsetX)),
                Math.Max(1, charSizeInfo.glyphHeight + charSizeInfo.offsetY));

            glyph.Mutate(context => context
                .Filter(_colorMatrices[charMap.imageInformation.colorChannel])
                .Clip(new RectangularPolygon(destRect), context1 => context1
                    .DrawImage(fontImage, srcRect, 1f)));

            return glyph;
        }

        private Stream Decompress(Stream input)
        {
            var output = new MemoryStream();

            Level5Compressor.Decompress(input, output);
            output.Position = 0;

            return output;
        }

        private void WriteMultipleCompressed<T>(BinaryWriterX bw, IList<T> list, Level5CompressionMethod comp)
        {
            var ms = new MemoryStream();
            using (var bwOut = new BinaryWriterX(ms, true))
                bwOut.WriteMultiple(list);

            var compressedStream = new MemoryStream();
            ms.Position = 0;
            Level5Compressor.Compress(ms, compressedStream, comp);

            compressedStream.Position = 0;
            compressedStream.CopyTo(bw.BaseStream);
        }

        private uint ConvertChar(uint character)
        {
            // Specially handle space
            if (character == 0x20)
                return 0x3000;

            // Convert all other letters
            if (character >= 0x21 && character <= 0x7E)
                return character + 0xFEE0;

            return character;
        }
    }
}
