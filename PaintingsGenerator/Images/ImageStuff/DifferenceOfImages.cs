namespace PaintingsGenerator.Images.ImageStuff {
    internal class DifferenceOfImages : Container<ushort> {
        public ulong SumDiff { get; private set; } = 0;
        public double ScaledDiff => SumDiff / Size;

        public DifferenceOfImages(int width, int height) : base(new ushort[height, width]) {
        }

        override public ushort this[int i, int j] {
            get => base[i, j];
            set {
                SumDiff -= base[i, j];
                SumDiff += value;
                base[i, j] = value;
            }
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
