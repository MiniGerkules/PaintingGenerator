namespace PaintingsGenerator {
    public record class Bounds {
        public int LeftX { get; init; }
        public int RightX { get; init; }
        public int UpY { get; init; }
        public int DownY { get; init; }

        public Bounds(int leftX, int rightX, int downY, int upY) {
            LeftX = leftX;
            RightX = rightX;
            UpY = upY;
            DownY = downY;
        }

        public bool XInBounds(int x) => LeftX <= x && x <= RightX;
        public bool YInBounds(int y) => DownY <= y && y <= UpY;
        public bool YInBounds(double y) => DownY <= y && y <= UpY;
        public bool InBounds(Position pos) => XInBounds(pos.X) && YInBounds(pos.Y);
    }
}
