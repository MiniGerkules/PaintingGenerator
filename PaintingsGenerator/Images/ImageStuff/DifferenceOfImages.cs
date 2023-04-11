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

        public ulong GetSumDifference(Position pos, uint height) {
            var part = GetCirclePart(pos, height);

            ulong diff = 0;
            foreach (var elem in part) {
                diff += elem;
            }

            return diff;
        }
    }
}
