using PaintingsGenerator.Colors;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal class GrayImage : Image<GrayColor> {
        public GrayImage(RGBImage rgbImage)
                : base(new GrayColor[rgbImage.Height, rgbImage.Width]) {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++)
                    this[y, x] = new(rgbImage[y, x]);
            }
        }

        public override void AddStroke(Stroke<GrayColor> stroke) {
            foreach (var pos in stroke.Positions)
                this[pos.Position.Y, pos.Position.X] = new(stroke.Color.Gray);
        }
    }
}
