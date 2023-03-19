namespace PaintingsGenerator.Images.ImageStuff {
    internal class DifferenceOfImages : Container<ushort> {
        public DifferenceOfImages(int width, int height) : base(new ushort[height, width]) {
        }

        public double SumDiff() {
            ulong diffSum = 0;

            for (int y = 0; y < Height; ++y) {
                for (int x = 0; x < Width; ++x)
                    diffSum += this[y, x];
            }

            return (double)diffSum / (Width*Height);
        }

        public double GetDifference(Position pos, uint height) {
            var part = GetPart(pos, height);

            ulong diff = 0;
            for (int y = 0; y < part.Height; ++y) {
                for (int x = 0; x < part.Width; ++x)
                    diff += part[y, x];
            }

            return (double)diff / part.Size;
        }
    }
}
