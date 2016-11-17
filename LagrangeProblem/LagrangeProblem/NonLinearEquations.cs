using System;

namespace LagrangeProblem
{
    class NonLinearEquation //инкапсулирует нелинейное уравнение и содержит метод хорд
    {
        //данные начальные точки для метода хорд
        readonly double previousStartingPoint;
        readonly double nextStartingPoint;

        //сама нелинейная функция
        readonly Func<double, double> F;

        public double ApplyMethodOfChords(double epsilon)
        {
            double previousPoint = previousStartingPoint;
            double nextPoint = nextStartingPoint;
            double nextValue = F(nextPoint);
            double previousValue = F(previousPoint);

            while (Math.Abs(nextValue) >= epsilon)
            {
                nextPoint = nextPoint - nextValue * (nextPoint - previousPoint) / (nextValue - previousValue);
                nextValue = F(nextPoint);
            }
            return nextValue;
        }

        public NonLinearEquation(double previousStartingPoint, double nextStartingPoint, Func<double, double> F)
        {
            this.previousStartingPoint = previousStartingPoint;
            this.nextStartingPoint = nextStartingPoint;
            this.F = F;
        }
    }
    class SystemOfNonLinearEquations //инкапсулирует систему нелинейных уравнений и содержит метод Ньютона
    {
        //начальное значение для метода Ньютона
        readonly Vector startingArgumentValue;

        //сама нелинейная векторная функция векторного аргумента
        readonly Func<Vector, Vector> F;

        //собственно, метод Ньютона
        public Vector ApplyMethodOfNewton(double epsilon)
        {
            Vector functionValue, argumentChange, currentArgumentValue;
            SquareMatrix jacobianMatrixValue, inverseJacobianMatrixValue;

            currentArgumentValue = startingArgumentValue;
            functionValue = F(currentArgumentValue);
            sbyte i = 0;
            while (functionValue.Length >= epsilon)
            {
                jacobianMatrixValue = GetJacobianMatrix(currentArgumentValue);
                inverseJacobianMatrixValue = jacobianMatrixValue.GetInverseMatrix();
                argumentChange = -(inverseJacobianMatrixValue * functionValue);
                currentArgumentValue += argumentChange;
                functionValue = F(currentArgumentValue);
                i++;
                //Если метод Ньютона не сходится, бросаем исключение
                if (i > 20) throw new NonLinearEquationsException("Method of Newton can't be applied.");
            }
            return currentArgumentValue;
        }
        SquareMatrix GetJacobianMatrix(Vector argument)
        {
            double h = 1e-5; //шаг для формулы центральной разности
            double[] argumentChange = new double[argument.Dimension]; //будем использовать для приращения
            //массив векторов, в котором каждый вектор является частной производной вектор-функции по одной переменной
            Vector[] partialDerivatives = new Vector[argument.Dimension];

            //для каждой переменной находим вектор частных производных
            for (sbyte i = 0; i < argument.Dimension; i++)
            {
                argumentChange[i] = h;
                partialDerivatives[i] = (F(argument + argumentChange) - F(argument - argumentChange)) * (1 / (2 * h));
                argumentChange[i] = 0.0;
            }
            //объединяем массив векторов в матрицу, которая и будет матрицей Якоби
            return new SquareMatrix(partialDerivatives);
        }
    }
    class NonLinearEquationsException : Exception
    {
        public NonLinearEquationsException() : base("Incorrect non-linear equation(s).") { }
        public NonLinearEquationsException(string message) : base(message) { }
    }
}