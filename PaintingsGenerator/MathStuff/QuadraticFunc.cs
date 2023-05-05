namespace PaintingsGenerator.MathStuff {
    internal record class QuadraticFunc(double A, double B, double C,
                                        double Curvative) {
        public double DerivativeAt(double x) => 2*A*x + B;
    }
}
