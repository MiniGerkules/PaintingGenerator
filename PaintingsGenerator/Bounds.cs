namespace PaintingsGenerator {
    public record class Bounds {
        public int LeftX { get; init; }
        public int RightX { get; init; }
        public int UpY { get; init; }
        public int DownY { get; init; }

        public Bounds(int leftX, int rightX, int upY, int downY) {
            LeftX = leftX;
            RightX = rightX;
            UpY = upY;
            DownY = downY;
        }

        public Bounds(Position pos1, Position pos2, uint radius) {
            if (pos1.X < pos2.X)
                (LeftX, RightX) = (pos1.X, pos2.X);
            else
                (LeftX, RightX) = (pos2.X, pos1.X);

            if (pos1.Y < pos2.Y)
                (UpY, DownY) = (pos1.Y, pos2.Y);
            else
                (UpY, DownY) = (pos2.Y, pos1.Y);
        }

        public bool XInBounds(int x) => LeftX <= x && x <= RightX;
        public bool YInBounds(int y) => UpY <= y && y <= DownY;
        public bool InBounds(Position pos) => XInBounds(pos.X) && YInBounds(pos.Y);
    }
}
