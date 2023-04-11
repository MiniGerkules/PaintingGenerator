using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    public abstract class Image<PixelColor> : Container<PixelColor> {
        public Image(PixelColor[,] pixels) : base(pixels) {
        }

        public abstract void AddStroke(Stroke<PixelColor> stroke);
    }
}
