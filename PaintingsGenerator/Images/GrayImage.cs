using System.Windows.Media;
using System.Windows.Media.Imaging;

using PaintingsGenerator.Colors;
using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    internal class GrayImage : Image<GrayColor> {
        public GrayImage(RGBImage rgbImage)
                : base(PixelFormats.Gray8, new GrayColor[rgbImage.Height, rgbImage.Width]) {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++)
                    this[y, x] = new(rgbImage[y, x]);
            }
        }

        public override void AddStroke(Stroke<GrayColor> stroke) {
            foreach (var pos in stroke.Positions)
                this[pos.Position.Y, pos.Position.X] = new(stroke.Color.Gray);
        }

        public override BitmapSource ToBitmap() {
            int stride = Width * BYTES_PER_PIXEL;
            var pixels = new byte[Height * stride];

            for (int y = 0; y < Height; ++y) {
                for (int x = 0; x < Width; ++x) {
                    var curPtr = y*Width + x;
                    pixels[curPtr] = this[y, x].Gray;
                }
            }

            return BitmapSource.Create(
                Width, Height, 96, 96,
                FORMAT, null,
                pixels, stride
            );
        }
    }
}
