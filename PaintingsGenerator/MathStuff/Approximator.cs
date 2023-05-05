using System.Collections.Immutable;

namespace PaintingsGenerator.MathStuff {
    internal class Approximator {
        public static LineFunc GetLinearApproximation(ImmutableList<Position> positions) {
            long xSum = 0, ySum = 0, x2Sum = 0, xySum = 0;

            foreach (var position in positions) {
                xSum += position.X;
                ySum += position.Y;
                x2Sum += position.X * position.X;
                xySum += position.X * position.Y;
            }

            long n = positions.Count;
            double k = (double)(n*xySum - xSum*ySum) / (n*x2Sum - xSum*xSum);
            double b = (double)(ySum*x2Sum - xSum*xySum) / (n*x2Sum - xSum*xSum);

            return new(k, b);
        }
    }
}
