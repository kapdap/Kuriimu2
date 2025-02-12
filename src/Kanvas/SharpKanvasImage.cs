using System;
using System.Collections.Generic;
using Kanvas.Encoding;
using Kontract;
using Kontract.Interfaces.Progress;
using Kontract.Kanvas;
using Kontract.Models.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kanvas;

public class SharpKanvasImage : IKanvasImage
{
	private Image<Rgba32> _image;

	public int BitDepth { get; }

	public EncodingDefinition EncodingDefinition { get; }

	public Kontract.Models.Image.ImageInfo ImageInfo => null;

	public bool IsIndexed => false;

	public int ImageFormat => 0;

	public int PaletteFormat => 0;

	public Size ImageSize => _image.Size;

	public string Name { get; }

	public bool IsImageLocked => true;

	public bool ContentChanged { get; set; }

	public SharpKanvasImage(Image<Rgba32> image)
	{
		IColorEncoding imageEncoding = GetImageEncoding(image);
		EncodingDefinition = new EncodingDefinition();
		EncodingDefinition.AddColorEncoding(0, imageEncoding);
		BitDepth = imageEncoding.BitDepth;
		_image = image;
	}

	public SharpKanvasImage(Image<Rgba32> image, string name)
		: this(image)
	{
		Name = name;
	}

	public Image<Rgba32> GetImage(IProgressContext progress = null)
	{
		return _image;
	}

	public void SetImage(Image<Rgba32> image, IProgressContext progress = null)
	{
		ContentChanged = true;
		_image = image;
	}

	public void TranscodeImage(int imageFormat, IProgressContext progress = null)
	{
		if (IsImageLocked)
		{
			throw new InvalidOperationException("Image cannot be transcoded to another format.");
		}
		throw new InvalidOperationException("Transcoding image is not supported for bitmaps.");
	}

	public IList<Rgba32> GetPalette(IProgressContext progress = null)
	{
		ContractAssertions.IsTrue(IsIndexed, "IsIndexed");
		throw new InvalidOperationException("Getting palette is not supported for bitmaps.");
	}

	public void SetPalette(IList<Rgba32> palette, IProgressContext progress = null)
	{
		ContractAssertions.IsTrue(IsIndexed, "IsIndexed");
		throw new InvalidOperationException("Setting palette is not supported for bitmaps.");
	}

	public void TranscodePalette(int paletteFormat, IProgressContext progress = null)
	{
		ContractAssertions.IsTrue(IsIndexed, "IsIndexed");
		throw new InvalidOperationException("Transcoding palette is not supported for bitmaps.");
	}

	public void SetColorInPalette(int paletteIndex, Rgba32 color)
	{
		ContractAssertions.IsTrue(IsIndexed, "IsIndexed");
		throw new InvalidOperationException("Setting color in palette is not supported for bitmaps.");
	}

	public void SetIndexInImage(Point point, int paletteIndex)
	{
		ContractAssertions.IsTrue(IsIndexed, "IsIndexed");
		throw new InvalidOperationException("Setting index in image is not supported for bitmaps.");
	}

	public void Dispose()
	{
		_image = null;
	}

	private IColorEncoding GetImageEncoding(Image<Rgba32> image)
	{
		return new Rgba(8, 8, 8, 8, "ARGB");
	}
}
