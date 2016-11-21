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
        readonly Vector analyticalSolutionForInitialParameter;
        readonly double initialParameter;

        //сама нелинейная векторная функция векторного аргумента
        readonly Func<Vector, double, double, Method, Vector> F;

        //собственно, метод Ньютона
        public Vector ApplyMethodOfNewton(double epsilon, Vector initialApproximation, double parameter, Method method)
        {
            Vector functionValue, pointChange, currentPoint;
            SquareMatrix jacobianMatrixValue, inverseJacobianMatrixValue;

            currentPoint = initialApproximation;
            functionValue = F(currentPoint, epsilon, parameter, method);
            sbyte i = 0;
            while (functionValue.Length >= epsilon)
            {
                //берем матрицу Якоби в данной точке
                jacobianMatrixValue = GetJacobianMatrix(currentPoint, epsilon, parameter, method);
                //берем обратную к матрице Якоби
                inverseJacobianMatrixValue = jacobianMatrixValue.GetInverseMatrix();
                //решаем систему линейных уравнений, где неизвестная - приращение аргумента
                pointChange = -(inverseJacobianMatrixValue * functionValue);
                //приращаем аргумент
                currentPoint += pointChange;
                //вычислям функцию уже в новой точке
                functionValue = F(currentPoint, epsilon, parameter, method);
                i++;
                //Если метод Ньютона не сходится, бросаем исключение
                if (i > 10) throw new NonLinearEquationsException("Method of Newton can't be applied.");
            }
            return currentPoint;
        }
        SquareMatrix GetJacobianMatrix(Vector argument, double epsilon, double parameter, Method method)
        {
            double h = 1e-5; //шаг для формулы центральной разности
            double[] argumentChange = new double[argument.Dimension]; //будем использовать для приращения
            //массив векторов, в котором каждый вектор является частной производной вектор-функции по одной переменной
            Vector[] partialDerivatives = new Vector[argument.Dimension];

            //для каждой переменной находим вектор частных производных
            for (sbyte i = 0; i < argument.Dimension; i++)
            {
                argumentChange[i] = h;
                partialDerivatives[i] =
                    (F(argument + argumentChange, epsilon, parameter, method) -
                        F(argument - argumentChange, epsilon, parameter, method)) * (1 / (2 * h));
                argumentChange[i] = 0.0;
            }
            //объединяем массив векторов в матрицу, которая и будет матрицей Якоби
            return new SquareMatrix(partialDerivatives);
        }
        public Vector ApplyParameterContinuationMethod(double epsilon, double finalParameter, Method method)
        {   
            if(finalParameter <= initialParameter)
            {
                throw new NonLinearEquationsException("Incorrect final parameter in parameter continuation method.");
            }
            double parameterChange = 1.0;
            double currentParameter = initialParameter;
            double nextParameter;
            Vector solutionForCurrentParameter = analyticalSolutionForInitialParameter;
            Vector solutionForNextParameter;
            sbyte i = 0;
            do
            {
                nextParameter = currentParameter + parameterChange;
                if (nextParameter > finalParameter)
                {
                    nextParameter = finalParameter;
                }
                try
                {
                    solutionForNextParameter = ApplyMethodOfNewton(epsilon, solutionForCurrentParameter, nextParameter, method);
                } catch(NonLinearEquationsException)
                {
                    parameterChange /= 2;
                    Console.WriteLine(parameterChange);
                    continue;
                }
                currentParameter = nextParameter;
                solutionForCurrentParameter = solutionForNextParameter;
                //parameterChange = 0.5;
                i++;
                if (i > 50) throw new NonLinearEquationsException("Parameter continuation method can't be applied.");
            } while (currentParameter < finalParameter - epsilon);

            return solutionForCurrentParameter;
        }
        public SystemOfNonLinearEquations(Vector analyticalSolutionForInitialParameter,
            double initialParameter, Func<Vector, double, double, Method, Vector> F)
        {
            this.analyticalSolutionForInitialParameter = analyticalSolutionForInitialParameter;
            this.initialParameter = initialParameter;
            this.F = F;
        }
    }
    class NonLinearEquationsException : Exception
    {
        public NonLinearEquationsException() : base("Incorrect non-linear equation(s).") { }
        public NonLinearEquationsException(string message) : base(message) { }
    }
}