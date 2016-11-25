using System;

namespace LagrangeProblem
{
    //инкапсулирует систему нелинейных уравнений и содержит метод Ньютона и метод продолжения по параметру
    class SystemOfNonLinearEquations
    {
        //начальное значение для метода Ньютона
        readonly Vector analyticalSolutionForInitialParameter;
        //параметр, при котором получено начальное значение
        readonly double initialParameter;

        //сама нелинейная векторная функция векторного аргумента
        readonly Func<Vector, double, double, Method, Vector> F;

        //собственно, метод Ньютона
        Vector ApplyMethodOfNewton(double epsilon,
            Vector initialApproximation, double parameter, Method method)
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
            if(finalParameter < initialParameter)
            {
                throw new NonLinearEquationsException("Incorrect final parameter in parameter continuation method.");
            }
            //начальный шаг берем самый большой (чтобы сразу достичь конечного параметра)
            double parameterChange = finalParameter - initialParameter;
            double currentParameter = initialParameter;
            double nextParameter;
            Vector solutionForCurrentParameter = analyticalSolutionForInitialParameter;
            Vector solutionForNextParameter;
            sbyte i = 0;
            do
            {
                nextParameter = currentParameter + parameterChange; //шагаем
                if (nextParameter > finalParameter) //чтоб не перешагнуть
                {
                    nextParameter = finalParameter;
                }
                try
                {
                    //пробуем применить метод Ньютона для данного шага
                    solutionForNextParameter = ApplyMethodOfNewton(epsilon,
                        solutionForCurrentParameter, nextParameter, method);
                }
                catch (Exception exception)
                {
                    //если попытка прошла неудачно, укорачиваем шаг в два раза
                    if(exception is NonLinearEquationsException || exception is ProblemException)
                    {
                        parameterChange /= 2;
                        continue;
                    }
                    throw;
                }
                currentParameter = nextParameter;
                solutionForCurrentParameter = solutionForNextParameter;
                //parameterChange = finalParameter;
                i++;
                //если метод продолжения по параметру не сходится, бросаем исключение
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