using System;

namespace PaintingsGenerator.MathStuff {
    internal record class LineFunc {
        public double K { get; init; }
        public double B { get; init; }

        public LineFunc(double k, double b) {
            K = k;
            B = b;
        }

        public LineFunc(double xStart, double yStart, double xEnd, double yEnd) {
            K = (double)(yEnd-yStart) / (xEnd-xStart);
            B = CountB(K, xStart, yStart);
        }

        public LineFunc(Position pos1, Position pos2) : this(pos1.X, pos1.Y, pos2.X, pos2.Y) {
        }

        public bool IsVertical() => double.IsInfinity(K);
        public bool IsHorizontal() => Math.Abs(K) <= 1e-5;

        public double CountX(double y) => (y-B) / K;
        public double CountY(double x) => K*x + B;

        public double GetKForPerp() => -1.0/K;
        public LineFunc GetPerp(Position goThrough) {
            return new(GetKForPerp(), CountB(GetKForPerp(), goThrough.X, goThrough.Y));
        }

        public static double CountK(Position pos1, Position pos2) => (double)(pos2.Y-pos1.Y) / (pos2.X - pos1.X);
        public static double CountB(double k, double xThrought, double yThrough) => yThrough - k*xThrought;
    }
}
