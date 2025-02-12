﻿using System;
using System.IO;
using Kanvas.Swizzle;
using Komponent.IO;
using SixLabors.ImageSharp;
using ImageInfo = Kontract.Models.Image.ImageInfo;

namespace plugin_mcdonalds.Images
{
    class ECDPNcgr
    {
        private NitroHeader _ncgrHeader;
        private NitroHeader _nclrHeader;
        private NitroCharHeader _charHeader;
        private NitroTtlpHeader _ttlpHeader;
        private byte[] _palette;

        public ImageInfo Load(Stream ncgrStream, Stream nclrStream)
        {
            using var ncgrBr = new BinaryReaderX(ncgrStream);
            using var nclrBr = new BinaryReaderX(nclrStream);

            // Read generic headers
            _ncgrHeader = ncgrBr.ReadType<NitroHeader>();
            _nclrHeader = nclrBr.ReadType<NitroHeader>();

            // Read Char header
            _charHeader = ncgrBr.ReadType<NitroCharHeader>();

            // Read Ttlp header
            _ttlpHeader = nclrBr.ReadType<NitroTtlpHeader>();

            // Read palette data
            byte[] paletteData;
            if (GetBitDepth(_charHeader.imageFormat) == 4)
            {
                nclrBr.BaseStream.Position += 0xE * 0x20;
                paletteData = nclrBr.ReadBytes(0x20);

                // Backup complete palette data
                nclrBr.BaseStream.Position -= 0xF * 20;
                _palette = nclrBr.ReadBytes(_ttlpHeader.paletteSize);
            }
            else
                paletteData = nclrBr.ReadBytes(_ttlpHeader.paletteSize);

            // Create image
            var dataLength = _charHeader.tileCountX < 0 ? _charHeader.tileDataSize : _charHeader.tileCountX * _charHeader.tileCountY;
            var data = ncgrBr.ReadBytes(dataLength);
            var size = GetImageSize(_charHeader);

            var imageInfo = new ImageInfo(data, _charHeader.imageFormat, size)
            {
                ImageFormat = _charHeader.imageFormat,

                PaletteData = paletteData,
                PaletteFormat = 0
            };

            imageInfo.RemapPixels.With(context => new NitroSwizzle(context));

            return imageInfo;
        }

        // Main logic taken from Tinke "Ekona/Images/Actions.cs Get_Size"
        private Size GetImageSize(NitroCharHeader header)
        {
            if (header.tileCountX > 0)
                return new Size(header.tileCountX * 8, header.tileCountY * 8);

            var pixelCount = header.tileDataSize * 8 / GetBitDepth(header.imageFormat);

            // If image is squared
            var sqrt = (int)Math.Sqrt(pixelCount);
            if ((int)Math.Pow(sqrt, 2) == pixelCount)
                return new Size(sqrt, sqrt);

            // Otherwise derive it from data size
            var width = Math.Min(pixelCount, 0x100);
            width = width == 0 ? 1 : width;

            var height = pixelCount / width;
            height = height == 0 ? 1 : height;

            return new Size(width, height);
        }

        private int GetBitDepth(int format)
        {
            switch (format)
            {
                case 3:
                    return 4;

                case 4:
                    return 8;

                default:
                    throw new InvalidOperationException($"Unsupported image format '{format}'.");
            }
        }
    }
}
