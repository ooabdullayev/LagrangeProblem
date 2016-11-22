using System;

namespace LagrangeProblem
{
    class SquareMatrix
    {
        readonly double[,] components; //для хранения элементов матрицы
        public sbyte Dimension //размерность квадратной матрицы
        {
            get
            {
                return checked((sbyte)(components.GetUpperBound(0) + 1));
            }
        }
        public SquareMatrix(sbyte dimension) //конструктор матрицы из заданной размерности
        {
            if (dimension < 1) throw new SquareMatrixException("Incorrect dimension of matrix.");
            components = new double[dimension, dimension];
        }
        public SquareMatrix(double[,] components) //конструктор матрицы из двумерного массива
        {
            if (components.GetUpperBound(0) != components.GetUpperBound(1))
                throw new SquareMatrixException("Matrix isn't square.");
            this.components = (double[,])components.Clone();
        }
        public SquareMatrix(Vector[] vectors)
        {
            foreach(Vector vector in vectors)
            {
                if(vector.Dimension != vectors.Length)
                    throw new SquareMatrixException("Vector dimensions aren't correct.");
            }
            components = new double[vectors.Length, vectors.Length];

            for(sbyte i = 0; i < vectors.Length; i++)
            {
                for(sbyte j = 0; j < vectors.Length; j++)
                {
                    components[i, j] = vectors[j][i];
                }
            }
        }
        public double this[sbyte index1, sbyte index2]
        {
            get
            {
                return components[index1, index2];
            }

        }
        public static SquareMatrix operator +(SquareMatrix op1, SquareMatrix op2)
        {
            if (op1.Dimension != op2.Dimension)
                throw new SquareMatrixException("Dimensions of matrices in + operator are not the same.");
            double[,] result = new double[op1.Dimension, op2.Dimension];
            for (sbyte i = 0; i < result.GetUpperBound(0) + 1; i++)
                for (sbyte j = 0; j < result.GetUpperBound(1) + 1; j++)
                {
                    result[i, j] = op1[i, j] + op2[i, j];
                }
            return new SquareMatrix(result);
        }
        public static SquareMatrix operator -(SquareMatrix op1, SquareMatrix op2)
        {
            if (op1.Dimension != op2.Dimension)
                throw new SquareMatrixException("Dimensions of matrices in - operator are not the same.");
            double[,] result = new double[op1.Dimension, op2.Dimension];
            for (sbyte i = 0; i < result.GetUpperBound(0) + 1; i++)
                for (sbyte j = 0; j < result.GetUpperBound(1) + 1; j++)
                {
                    result[i, j] = op1[i, j] - op2[i, j];
                }
            return new SquareMatrix(result);
        }

        public static SquareMatrix operator *(double op1, SquareMatrix op2) //умножение числа на матрицу
        {
            double[,] result = new double[op2.Dimension, op2.Dimension];
            for (sbyte i = 0; i < result.GetUpperBound(0) + 1; i++)
                for (sbyte j = 0; j < result.GetUpperBound(1) + 1; j++)
                {
                    result[i, j] = op1 * op2[i, j];
                }
            return new SquareMatrix(result);
        }
        public static SquareMatrix operator *(SquareMatrix op1, double op2) //умножение матрицы на число
        {
            return op2 * op1;
        }
        public static SquareMatrix operator *(SquareMatrix op1, SquareMatrix op2) //произведение двух матриц
        {
            if (op1.Dimension != op2.Dimension)
                throw new SquareMatrixException("Dimensions of matrices in * operator are not the same.");
            double[,] result = new double[op1.Dimension, op2.Dimension];
            double accumulator;

            for (sbyte i = 0; i < result.GetUpperBound(0) + 1; i++)
                for (sbyte j = 0; j < result.GetUpperBound(1) + 1; j++)
                {
                    accumulator = 0.0;
                    for (sbyte l = 0; l < result.GetUpperBound(0) + 1; l++)
                    {
                        accumulator += op1[i, l] * op2[l, j];
                    }
                    result[i, j] = accumulator;
                }
            return new SquareMatrix(result);
        }
        public static Vector operator *(Vector op1, SquareMatrix op2) //умножение вектора слева на матрицу
        {
            if (op1.Dimension != op2.Dimension)
                throw new SquareMatrixException("Dimensions of matrix and vector in * operator are not the same.");
            double[] result = new double[op1.Dimension];
            double accumulator;

            for (sbyte i = 0; i < result.Length; i++)
            {
                accumulator = 0.0;
                for (sbyte j = 0; j < result.Length; j++)
                {
                    accumulator += op1[j] * op2[j, i];
                }
                result[i] = accumulator;
            }
            return new Vector(result);
        }
        public static Vector operator *(SquareMatrix op1, Vector op2) //умножение вектора справа на матрицу
        {
            if (op1.Dimension != op2.Dimension)
                throw new SquareMatrixException("Dimensions of matrix and vector in * operator are not the same.");
            double[] result = new double[op2.Dimension];
            double accumulator;

            for (sbyte i = 0; i < result.Length; i++)
            {
                accumulator = 0.0;
                for (sbyte j = 0; j < result.Length; j++)
                {
                    accumulator += op1[i, j] * op2[j];
                }
                result[i] = accumulator;
            }
            return new Vector(result);
        }
        //Метод Гаусса-Жордана получения матрицы, обратной к данной (со строками)
        public SquareMatrix GetInverseMatrix()
        {
            double[,] ourMatrix = (double[,])components.Clone(); //данная матрица
            double[,] identityMatrix = CreateIdentityMatrix(Dimension); //единичная матрица

            for(sbyte i = 0; i < Dimension; i++)
            {
                //ищем ниже диагонали в данном столбце индекс максимального по модулю элемента
                sbyte indexOfMaxValueInColumn = GetIndexOfMaxValueInColumn(i, ourMatrix);
                //если находим, то меняем местами текущую строку и ту, в индекс которой нашли
                if(i != indexOfMaxValueInColumn)
                {
                    //поменять местами i-ю строку с indexOfMaxValueInColumn-ой строкой в матрице ourMatrix
                    SwapRows(i, indexOfMaxValueInColumn, ourMatrix);
                    SwapRows(i, indexOfMaxValueInColumn, identityMatrix);
                }
                //на это число, находящееся на диагонали мы будем делить всю текущую строку
                double divider = ourMatrix[i, i];
                //делим i-ю строку матрицы ourMatrix на divider
                Divide(ourMatrix, i, divider);
                Divide(identityMatrix, i, divider);
                //теперь вычитаем из всех строк текущую, умноженную на соответсвующий коэффициент (кроме текущей)
                for(sbyte j = 0; j < Dimension; j++)
                {
                    if (j == i) continue; //кроме текущей
                    double coefficient = ourMatrix[j, i];
                    //вычесть из j-ой строки i-ю, умноженную на coefficient
                    Subtract(ourMatrix, j, i, coefficient);
                    Subtract(identityMatrix, j, i, coefficient);
                }
            }
            //полученная из единичной матрица и будет обратной, ее и возвращаем
            return new SquareMatrix(identityMatrix);
        }
        double[,] CreateIdentityMatrix(sbyte dimension) //создает и возвращает единичную матрицу заданной размерности
        {
            double[,] result = new double[dimension, dimension];
            for(sbyte i = 0; i < dimension; i++)
            {
                result[i, i] = 1.0;
            }
            return result;
        }
        //ищет индекс строки с максимальным значением среди элементов данного столбца, расположенных ниже диагонали
        sbyte GetIndexOfMaxValueInColumn(sbyte column, double[,] matrix)
        {
            sbyte indexOfMaxValueInColumn = column;

            for(sbyte i = checked((sbyte)(column + 1)); i < Dimension; i++)
            {
                if(Math.Abs(matrix[i, column]) > Math.Abs(matrix[indexOfMaxValueInColumn, column]))
                {
                    indexOfMaxValueInColumn = i;
                }
            }
            return indexOfMaxValueInColumn;
        }
        void SwapRows(sbyte firstRow, sbyte secondRow, double[,] matrix)
        {
            for(sbyte i = 0; i < Dimension; i++)
            {
                double keptValue = matrix[firstRow, i];
                matrix[firstRow, i] = matrix[secondRow, i];
                matrix[secondRow, i] = keptValue;
            }
        }
        void Divide(double[,] matrix, sbyte row, double divider)
        {
            for(sbyte i = 0; i < Dimension; i++)
            {
                matrix[row, i] /= divider;
            }
        }
        void Subtract(double[,] matrix, sbyte minuendRow, sbyte subtrahendRow, double coefficient)
        {
            for(sbyte i = 0; i < Dimension; i++)
            {
                matrix[minuendRow, i] -= matrix[subtrahendRow, i] * coefficient;
            }
        }

        //чтобы матрица печаталась красиво при интерпретации как строки
        public override string ToString()
        {
            string result = "";
            for(sbyte i = 0; i < Dimension; i++)
            {
                for(sbyte j = 0; j < Dimension; j++)
                {
                    result += components[i, j] + " ";
                }
                result += "\n";
            }
            return result;
        }
    }
    class SquareMatrixException : Exception
    {
        public SquareMatrixException() : base("Incorrect matrix.") { }
        public SquareMatrixException(string message) : base(message) { }
    }
}