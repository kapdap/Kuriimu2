using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace Kontract.Models.Layout
{
    public class RootLayoutElement : LayoutElement
    {
        public IList<LayoutElement> Children { get; } = new List<LayoutElement>();

        public RootLayoutElement(Size size, Point location) : base(size, location)
        {
        }

        public override void Draw(Image<Rgba32> gr,bool drawBorder)
        {
            base.Draw(gr, drawBorder);
            foreach (var child in Children)
            {
                child.Draw(gr, drawBorder);
            }
        }

        public override Point GetAbsoluteLocation()
        {
            return new Point(RelativeLocation.X, RelativeLocation.Y);
        }
    }
}
