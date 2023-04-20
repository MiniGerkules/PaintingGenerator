using System;

namespace PaintingsGenerator.MathStuff {
    internal struct Vector2D {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Length { get; }

        public Vector2D(double x, double y) {
            X = x;
            Y = y;
            Length = Math.Sqrt(X*X + Y*Y);
        }

        public Vector2D(Position start, Position end) : this(end.X - start.X, end.Y - start.Y) {
        }

        public Vector2D Reverse() => new(-X, -Y);

        public void Normalize() {
            double length = Math.Sqrt(X*X + Y*Y);

            X /= length;
            Y /= length;
        }

        public bool IsPoint() => X.Equals(0.0) && Y.Equals(0.0);
    }
}
