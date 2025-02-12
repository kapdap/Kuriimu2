using SixLabors.ImageSharp.PixelFormats;

namespace Kanvas.Extensions;

public static class ColorExtensions
{
	public static float GetHue(this Rgba32 rgba)
	{
        if (rgba.R == rgba.G && rgba.G == rgba.B)
            return 0; // 0 makes as good an UNDEFINED value as any

        float r = rgba.R / 255.0f;
        float g = rgba.G / 255.0f;
        float b = rgba.B / 255.0f;

        float max, min;
        float delta;
        float hue = 0.0f;

        max = r; min = r;

        if (g > max) max = g;
        if (b > max) max = b;

        if (g < min) min = g;
        if (b < min) min = b;

        delta = max - min;

        if (r == max)
        {
            hue = (g - b) / delta;
        }
        else if (g == max)
        {
            hue = 2 + (b - r) / delta;
        }
        else if (b == max)
        {
            hue = 4 + (r - g) / delta;
        }
        hue *= 60;

        if (hue < 0.0f)
        {
            hue += 360.0f;
        }
        return hue;
    }

	public static float GetBrightness(this Rgba32 rgba)
	{
        float r = rgba.R / 255.0f;
        float g = rgba.G / 255.0f;
        float b = rgba.B / 255.0f;

        float max, min;

        max = r; min = r;

        if (g > max) max = g;
        if (b > max) max = b;

        if (g < min) min = g;
        if (b < min) min = b;

        return (max + min) / 2;
    }

	public static float GetSaturation(this Rgba32 rgba)
	{
        float r = rgba.R / 255.0f;
        float g = rgba.G / 255.0f;
        float b = rgba.B / 255.0f;

        float max, min;
        float l, s = 0;

        max = r; min = r;

        if (g > max) max = g;
        if (b > max) max = b;

        if (g < min) min = g;
        if (b < min) min = b;

        // if max == min, then there is no color and
        // the saturation is zero.
        //
        if (max != min)
        {
            l = (max + min) / 2;

            if (l <= .5)
            {
                s = (max - min) / (max + min);
            }
            else
            {
                s = (max - min) / (2 - max - min);
            }
        }
        return s;
    }
}
