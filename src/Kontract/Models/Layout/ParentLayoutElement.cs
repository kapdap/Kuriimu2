using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Kontract.Models.Layout
{
    public class ParentLayoutElement : LeafLayoutElement
    {
        public IList<LayoutElement> Children { get; } = new List<LayoutElement>();

        public ParentLayoutElement(Size size, Point location, LayoutElement parent) : base(size, location, parent)
        {
        }

        public override void Draw(Image<Rgba32> img, bool drawBorder)
        {
            base.Draw(img, drawBorder);
            foreach (var child in Children)
            {
                child.Draw(img, drawBorder);
            }
        }
    }
}
