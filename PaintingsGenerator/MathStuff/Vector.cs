using System;
using System.Linq;

namespace PaintingsGenerator.MathStuff {
    public class Vector {
        private readonly double[] vector;
        public int Size { get => vector.Length; }
        public bool IsRow { get; private set; }

        public double this[int index] {
            get => vector[index];
            set => vector[index] = value;
        }

        public Vector(int size, bool isRow = true) {
            if (size == 0)
                throw new ArgumentException("Can't create an empty vector!");

            vector = new double[size];
            IsRow = isRow;
        }

        public Vector(double[] vector, bool isRow = true) {
            if (vector.Length == 0)
                throw new ArgumentException("Can't create an empty vector!");

            this.vector = new double[vector.Length];
            IsRow = isRow;
            Array.Copy(vector, this.vector, vector.Length);
        }

        public Vector(Vector other) : this(other.vector, other.IsRow) {
        }

        public void Transpose() {
            IsRow = !IsRow;
        }

        public void Abs() {
            for (int i = 0; i < Size; ++i)
                vector[i] = Math.Abs(vector[i]);
        }

        public void Norm() {
            var sum = vector.Sum();
            for (int i = 0; i < vector.Length; ++i)
                vector[i] /= sum;
        }

        internal static double ScalarProd(Vector a, Vector b) {
            if (a.Size != b.Size) throw new Exception("Vectors must be the same size!");

            var prod = 0.0;
            for (int i = 0; i < a.Size; ++i)
                prod += a[i] * b[i];

            return prod;
        }

        public static Vector operator *(Vector vec, double num) {
            var newVec = new Vector(vec);

            for (int i = 0; i < newVec.Size; ++i)
                newVec[i] *= num;

            return newVec;
        }

        public static Vector operator *(double num, Vector vec) => vec * num;

        public static Vector operator-(Vector a, Vector b) {
            if (a.Size != b.Size) throw new Exception("Vectors must be the same size!");

            var newVec = new Vector(a.Size);
            for (int i = 0; i < a.Size; ++i) {
                newVec[i] = a[i] - b[i];
            }

            return newVec;
        }
    }
}
