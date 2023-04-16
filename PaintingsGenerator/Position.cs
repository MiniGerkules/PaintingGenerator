namespace PaintingsGenerator {
    public record struct Position {
        public int X { get; init; }
        public int Y { get; init; }

        public Position(int x, int y) {
            X = x; Y = y;
        }
    }
}
