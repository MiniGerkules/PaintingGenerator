using PaintingsGenerator.Images.ImageStuff;

namespace PaintingsGenerator.Images {
    public abstract class Image<PixelColor> {
        protected readonly PixelColor[,] pixels;

        public int Height => pixels.GetLength(0);
        public int Width => pixels.GetLength(1);
        public PixelColor this[int i, int j] {
            get => pixels[i, j];
            set => pixels[i, j] = value;
        }

        protected Stroke<PixelColor>? lastStroke = null;

        public Image(PixelColor[,] pixels) {
            this.pixels = pixels;
        }

        public abstract void AddStroke(Stroke<PixelColor> stroke);
    }
}
