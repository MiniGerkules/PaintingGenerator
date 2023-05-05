using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PaintingsGenerator.MathStuff {
    public class Matrix2D {
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        private readonly Vector[] matrix;

        public double this[int row, int column] {
            get {
                if (matrix[0].IsRow)
                    return matrix[row][column];
                else
                    return matrix[column][row];
            }
            set {
                if (matrix[0].IsRow)
                    matrix[row][column] = value;
                else
                    matrix[column][row] = value;
            }
        }

        public double this[int index] {
            get {
                if (Rows == 1)
                    return matrix[0][index];
                else if (Columns == 1)
                    return matrix[index][0];
                else
                    throw new IndexOutOfRangeException("Index out of range!");
            }

            set {
                if (Rows == 1)
                    matrix[0][index] = value;
                else if (Columns == 1)
                    matrix[index][0] = value;
                else
                    throw new IndexOutOfRangeException("Index out of range!");
            }
        }

        public Matrix2D(int rows, int columns) {
            if (rows == 0 || columns == 0)
                throw new ArgumentException("Can't create an empty matrix!");

            Rows = rows;
            Columns = columns;
            matrix = new Vector[rows];
            for (int i = 0; i < rows; ++i)
                matrix[i] = new(columns);
        }

        public Matrix2D(int rows, int columns, double initVal) : this(rows, columns) {
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                    this[i, j] = initVal;
        }

        public Matrix2D(Vector vector) {
            if (vector.IsRow) {
                Rows = 1;
                Columns = vector.Size;
            } else {
                Rows = vector.Size;
                Columns = 1;
            }

            matrix = new Vector[] { new(vector) };
        }

        public Matrix2D(Matrix2D matrix) {
            Rows = matrix.Rows;
            Columns = matrix.Columns;
            this.matrix = new Vector[Rows];

            for (int i = 0; i < Rows; ++i)
                this.matrix[i] = new(matrix.matrix[i]);
        }

        public Vector GetRow(int rowIndex) {
            double[] vector = new double[Columns];
            for (int i = 0; i < Columns; ++i)
                vector[i] = this[rowIndex, i];

            return new(vector);
        }

        public void SetColumn(int index, Vector newColumn) {
            if (newColumn.IsRow) throw new ArgumentException("Must be column!");
            if (newColumn.Size != Rows) throw new ArgumentException("Wrong size!");
            
            if (!matrix.First().IsRow) {
                matrix[index] = newColumn;
            } else {
                for (int i = 0; i < newColumn.Size; ++i)
                    this[i, index] = newColumn[i];
            }
        }

        public Vector GetColumn(int columnIndex) {
            double[] vector = new double[Rows];
            for (int i = 0; i < Rows; ++i)
                vector[i] = this[i, columnIndex];

            return new(vector, false);
        }

        public Matrix2D Transpose() {
            Matrix2D result = new(this);

            (result.Rows, result.Columns) = (result.Columns, result.Rows);
            foreach (var vector in result.matrix)
                vector.Transpose();

            return result;
        }

        public Matrix2D MakeDiag() {
            if (!(Rows > 1 && Columns == 1 || Rows == 1 && Columns > 1))
                throw new ArgumentException("Can't make a diagonal matrix! The matrix must be a vector!");

            int dim = Math.Max(Rows, Columns);
            Matrix2D result = new(dim, dim, 0);

            for (int i = 0; i < dim; ++i)
                result[i, i] = this[i];

            return result;
        }

        /// <summary>
        /// The method defines the minus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of minux operation </returns>
        public static Matrix2D operator -(Matrix2D first, Matrix2D second) => ByElem(first, second, MathFunctions.Minus);

        /// <summary>
        /// The method defines the plus operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of plus operation </returns>
        public static Matrix2D operator +(Matrix2D first, Matrix2D second) => ByElem(first, second, MathFunctions.Plus);

        /// <summary>
        /// The method defines the element by element multiply operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of element by element multiply operation </returns>
        public static Matrix2D operator ^(Matrix2D first, Matrix2D second) => ByElem(first, second, MathFunctions.Multiply);

        /// <summary>
        /// The method defines the multyply between a matrix and a number
        /// </summary>
        /// <param name="matrix"> The matrix </param>
        /// <param name="number"> The number to multiply the matrix by </param>
        /// <returns></returns>
        public static Matrix2D operator *(Matrix2D matrix, double number) {
            Matrix2D result = new(matrix.Rows, matrix.Columns);

            for (int i = 0; i < result.Rows; ++i)
                for (int j = 0; j < result.Columns; ++j)
                    result[i, j] = number * matrix[i, j];

            return result;
        }

        /// <summary>
        /// The method defines the multyply between a matrix and a vector
        /// </summary>
        /// <param name="matrix"> The matrix </param>
        /// <param name="number"> The number to multiply the matrix by </param>
        /// <returns></returns>
        public static Matrix2D operator *(Matrix2D matrix, Vector vector) {
            if (!vector.IsRow && matrix.Columns != vector.Size)
                throw new ArgumentException("The number of rows first " +
                    "matrix don't equal the number of columns second matrix!");
            else if (vector.IsRow && matrix.Columns != 1)
                throw new ArgumentException("The number of rows first " +
                    "matrix don't equal the number of columns second matrix!");

            Matrix2D result;
            int rows = matrix.Rows;

            if (vector.IsRow) {
                int columns = vector.Size;
                result = new(rows, columns);

                for (int i = 0; i < rows; ++i)
                    for (int j = 0; j < columns; ++j)
                        result[i, j] = matrix[i, j] * vector[j];
            } else {
                int jMax = matrix.Columns;
                result = new(rows, 1, 0);

                for (int i = 0; i < rows; ++i)
                    for (int j = 0; j < jMax; ++j)
                        result[i] += matrix[i, j] * vector[j];
            }

            return result;
        }

        /// <summary>
        /// The method defines the matrix multiply operation
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> The result of matrix multiply operation </returns>
        public static Matrix2D operator *(Matrix2D first, Matrix2D second) {
            if (first.Columns != second.Rows)
                throw new ArgumentException("The number of rows first " +
                    "matrix don't equal the number of columns second matrix!");

            Matrix2D result = new(first.Rows, second.Columns, 0);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < second.Columns; ++j)
                    for (int k = 0; k < second.Rows; ++k)
                        result[i, j] += first[i, k] * second[k, j];

            return result;
        }

        /// <summary>
        /// The method defines the matrix element - by - element division
        ///  (sizes of matrixes should match)
        /// </summary>
        /// <param name="first"> First matrix </param>
        /// <param name="second"> Second matrix </param>
        /// <returns> ResultMatrix[i,j] = Matrix1[i,j] / Matrix2[i, j] </returns>
        public static Matrix2D operator /(Matrix2D first, Matrix2D second) {
            if (first.Rows != second.Rows || first.Columns != second.Columns)
                throw new ArgumentException("Matrixes sizes don't match!");

            Matrix2D result = new(first.Rows, second.Columns);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < second.Columns; ++j)
                    result[i, j] = first[i, j] / second[i, j];

            return result;
        }

        public static Matrix2D operator /(double num, Matrix2D matrix) {
            Matrix2D result = new(matrix.Rows, matrix.Columns);

            for (int i = 0; i < matrix.Rows; ++i)
                for (int j = 0; j < matrix.Columns; ++j)
                    result[i, j] = num / matrix[i, j];

            return result;
        }

        /// <summary>
        /// The method defines division matrix by number
        /// </summary>
        /// <param name="matrix"> The matrix </param>
        /// <param name="number"> The number to divide by </param>
        /// <returns></returns>
        public static Matrix2D operator /(Matrix2D matrix, double number) {
            return matrix * (1 / number);
        }

        private static Matrix2D ByElem(Matrix2D first, Matrix2D second,
            Func<double, double, double> action) {
            if (first.Rows != second.Rows || first.Columns != second.Columns)
                throw new ArgumentException("Dimensions of matrixes is different!");

            Matrix2D result = new(first.Rows, first.Columns);
            for (int i = 0; i < first.Rows; ++i)
                for (int j = 0; j < first.Columns; ++j)
                    result[i, j] = action(first[i, j], second[i, j]);

            return result;
        }
    }
}
