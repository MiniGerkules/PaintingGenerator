using System;

namespace PaintingsGenerator.MathStuff {
    internal record class QuadraticFunc(double A, double B, double C) : IFunc {
        public double DerivativeAt(double x) => 2*A*x + B;

        public double CountY(double x) => A*x*x + B*x + C;
        public double CountX(double y) {
            var newC = C - y;
            var D = B*B - 4*A*newC;
            return (-B+Math.Sqrt(D)) / (2*A);
        }
    }
}
