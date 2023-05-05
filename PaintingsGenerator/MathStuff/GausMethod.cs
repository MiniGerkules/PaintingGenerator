using System;

namespace PaintingsGenerator.MathStuff {
    public class GausMethod {
        public static Vector Solve(Matrix2D coefs, Matrix2D answers) {
            if (answers.Columns != 1 || answers.Rows != coefs.Rows)
                throw new ArgumentException("Can't solve a linear system! Matrix2D dimensions isn't same!");

            if (coefs.Rows != coefs.Columns)
                throw new ArgumentException("Method solve only a square matrix!");

            Matrix2D coefsCopy = new(coefs);
            Matrix2D answersCopy = new(answers);

            // Tikhonov regularization
            Matrix2D regularization = new(coefs.Rows, 1, 1e-10);
            regularization = regularization.MakeDiag();
            coefsCopy = coefsCopy + regularization;

            for (int i = 0; i < coefsCopy.Rows; ++i) {
                if (IsFullNulls(coefsCopy, i))
                    throw new ArgumentException("The system don't have one solution!");

                if (Equals(coefsCopy[i, i], 0))
                    if (!TrySwap(coefsCopy, answersCopy, i, i))
                        continue;

                for (int j = i + 1; j < coefs.Rows; ++j) {
                    double toMul = coefsCopy[j, i] / coefsCopy[i, i];
                    coefsCopy[j, i] = 0;
                    answersCopy[j] -= answersCopy[i] * toMul;

                    for (int k = i + 1; k < coefsCopy.Columns; ++k)
                        coefsCopy[j, k] -= coefsCopy[i, k] * toMul;
                }
            }

            Vector result = new(coefs.Columns);
            for (int i = result.Size - 1; i >= 0; i--) {
                if (Equals(coefsCopy[i, i], 0))
                    throw new ArgumentException("The system don't have one solution!");

                double divisible = answersCopy[i];
                for (int j = i + 1; j < coefs.Columns; ++j)
                    divisible -= result[j] * coefsCopy[i, j];

                result[i] = divisible / coefsCopy[i, i];
            }

            return result;
        }

        private static bool IsFullNulls(Matrix2D matrix, int curRow) {
            for (int i = 0; i < matrix.Columns; ++i)
                if (matrix[curRow, i] != 0)
                    return false;

            return true;
        }

        private static bool TrySwap(Matrix2D coefs, Matrix2D answers, int startRow, int column) {
            for (int i = startRow; i < coefs.Rows; ++i) {
                if (Equals(coefs[i, column], 0))
                    continue;

                for (int j = 0; j < coefs.Columns; ++j)
                    (coefs[i, j], coefs[startRow, j]) = (coefs[startRow, j], coefs[i, j]);
                (answers[i], answers[startRow]) = (answers[startRow], answers[i]);

                return true;
            }

            return false;
        }
    }
}
