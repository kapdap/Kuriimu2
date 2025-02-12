using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kontract.Models.Layout
{
    public abstract class LayoutElement
    {
        public Size Size { get; }

        public Point RelativeLocation { get; }

        protected LayoutElement(Size size, Point location)
        {
            Size = size;
            RelativeLocation = location;
        }

        public virtual void Draw(Image<Rgba32> img, bool drawBorder)
        {
            if (drawBorder)
                img.Mutate(i => i.Draw(new SolidPen(Color.Black, 2), new RectangleF(GetAbsoluteLocation(), Size)));

        }

        public abstract Point GetAbsoluteLocation();
    }
}
