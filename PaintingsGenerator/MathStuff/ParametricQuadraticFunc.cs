using System;
using System.Collections.Immutable;
using System.Linq;

namespace PaintingsGenerator.MathStuff {
    internal class ParametricQuadraticFunc : IFunc {
        private readonly QuadraticFunc xFunc;
        private readonly QuadraticFunc yFunc;
        public ImmutableArray<double> Parameter { get; }

        public ParametricQuadraticFunc(QuadraticFunc xFunc, QuadraticFunc yFunc,
                                       double[] parameter) {
            this.xFunc = xFunc;
            this.yFunc = yFunc;
            Parameter = parameter.ToImmutableArray();
        }

        public double CountX(double t) => xFunc.CountY(t);
        public double CountY(double t) => yFunc.CountY(t);

        public double DerivativeAt(double t) => yFunc.DerivativeAt(t) / xFunc.DerivativeAt(t);

        public double GetCurvative() {
            double ax = Math.Abs(xFunc.A), bx = Math.Abs(xFunc.B), cx = Math.Abs(xFunc.C);
            double ay = Math.Abs(yFunc.A), by = Math.Abs(yFunc.B), cy = Math.Abs(yFunc.C);
            
            var Np = Parameter.Length;
            var param2 = Parameter.Select(elem => elem*elem).ToArray();

            var sparams = new double[] {
                param2.Select(d => d * ax).Sum()/Np + param2.Select(d => d * ay).Sum()/Np,
                Parameter.Select(d => d * bx).Sum()/Np + Parameter.Select(d => d * by).Sum()/Np,
                cx + cy
            };

            return sparams[0] / sparams.Sum();
        }
    }
}
