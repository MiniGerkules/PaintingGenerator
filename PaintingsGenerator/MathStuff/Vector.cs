using System;

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

        internal void Abs() {
            for (int i = 0; i < Size; ++i)
                vector[i] = Math.Abs(vector[i]);
        }
    }
}
