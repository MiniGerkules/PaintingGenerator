namespace PaintingsGenerator {
    internal class Gradient {
        private readonly double[,] dx;
        private readonly double[,] dy;

        public Gradient(double[,] dx, double[,] dy) {
            this.dx = dx;
            this.dy = dy;
        }
    }
}
