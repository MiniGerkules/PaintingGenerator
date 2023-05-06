using System;
using System.Linq;
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

        public static ParametricQuadraticFunc GetQuadraticApproximation(ImmutableList<Position> positions) {
            // LSM in parametric coordinates
            int Np = positions.Count;

            // sort by distanse from first point
            var d2 = positions
                        .Select(pos => Math.Pow(pos.X - positions[0].X, 2) +
                                       Math.Pow(pos.Y - positions[0].Y, 2))
                        .ToArray();
            var indexes = Enumerable.Range(0, positions.Count).ToArray();
            Array.Sort(d2, indexes);

            var t = Enumerable.Range(0, Np).Select(elem => (double)elem / (Np - 1)).ToArray();
            var t2 = t.Select(elem => elem*elem).ToArray();

            var xSort = new double[Np];
            var ySort = new double[Np];
            for (int i = 0; i < xSort.Length; ++i) {
                xSort[i] = positions[indexes[i]].X;
                ySort[i] = positions[indexes[i]].Y;
            }

            Matrix2D E = new(positions.Count, 3, 1);
            E.SetColumn(1, new(t, false));
            E.SetColumn(2, new(t2, false));

            // for x
            var ET = E.Transpose();
            var V = new Vector(xSort, false);
            var coefs = ET * E;
            var answers = ET * V;
            var hx = GausMethod.Solve(coefs, answers);

            // for y
            V = new Vector(ySort, false);
            answers = ET * V;
            var hy = GausMethod.Solve(coefs, answers);

            return new(new(hx[2], hx[1], hx[0]), new(hy[2], hy[1], hy[0]), t);
        }
    }
}
