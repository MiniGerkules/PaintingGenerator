namespace PaintingsGenerator {
    public struct Position {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y) {
            X = x; Y = y;
        }

        public bool InBounds(int leftX, int upY, int rightX, int downY) {
            return leftX <= X && X <= rightX && upY <= Y && Y <= downY;
        }
    }
}
