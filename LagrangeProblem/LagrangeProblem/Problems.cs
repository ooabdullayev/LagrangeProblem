using System;
using System.Collections.Generic;

namespace LagrangeProblem
{
    class Problem //инкапсулирует систему уравнений с начальными условиями
    {
        public readonly sbyte numOfEquations; //число дифференциальных уравнений первого порядка в системе
        public readonly Func<double, Vector, double, Vector> f; //векторная функция, стоящая в правой части системы уравнений
        public readonly Func<double, Vector, double> Lambda; //функция от t, используюящаяся в вычислении глобальной погрешности

        public Results Solve(Method method, Points points, Conditions conditions, double eps, double parameter)
        {
            if (conditions.t0 >= points[0]) throw new ProblemException("Points and conditions are not compatible.");
            double h = 1;
            Vector y = conditions.y0; //начальное значение функции y берется в точке t = tMin
            double errGlobal = 0.0;
            double t = conditions.t0;
            List<Result> results = new List<Result>(); //список для накапливания результатов для каждой точки
            foreach (double point in points) //за каждую итерацию будем получать результат в очередной точке
            {
                y = GetNextValue(method, point, y, eps, ref h, ref t, ref errGlobal, parameter);
                results.Add(new Result(t, y, errGlobal));
            }
            return new Results(results, eps);
        }
        public Result Solve(Method method, double point, Conditions conditions, double eps, double parameter)
        {
            double h = 1;
            double errGlobal = 0.0;
            double t = conditions.t0;
            Vector y = GetNextValue(method, point, conditions.y0, eps, ref h, ref t, ref errGlobal, parameter);
            return new Result(t, y, errGlobal);
        }
        Vector GetNextValue(Method method, double point,
            Vector y, double eps, ref double h, ref double t, ref double errGlobal, double parameter)
        {
            if (point <= t) throw new ProblemException("Next point must be more than current.");
            Vector yChange; //соответствует "дельта игрик", содержит приращение функции y
            Vector y_Change; //приращение для "игрик с крышкой"
            double errLocal;
            double lambda;
            while (t < point - eps) //каждую итерацию корректируем шаг, и если шаг хороший, шагаем
            {
                
                if (t + h > point) h = point - t; //чтобы случайно не перешагнуть следующюю точку
                SetChanges(method, out yChange, out y_Change, h, y, t, parameter); //получаем приращения для y и y с крышкой
                errLocal = (yChange - y_Change).Length;
                if (errLocal < eps)
                {
                    lambda = Lambda(t, y); //вычисляем её именно здесь, так как нужно значение в предыдущей точке
                    t += h; //переходим к следующей точке
                    y += yChange; //получаем значение в следующей точке
                    errGlobal = errLocal + errGlobal * Math.Pow(Math.E, lambda * h);
                }
                h = GetHNew(method, errLocal, eps, h);
            }
            return y;
        }

        Vector[] GetK(Method method, double h, Vector y, double t, double parameter) //возвращает числа k1, k2, ..., ks для явного метода РК
        {
            Vector temp; //вспомогательная переменная для хранения частичной суммы
            Vector[] k = new Vector[method.numOfSteps]; //массив векторов k1, k2, ..., ks, в нашем случае s = numOfSteps
            k[0] = f(t, y, parameter);
            for (sbyte i = 1; i < k.Length; i++) //каждую итерацию получаем очередной ki
            {
                temp = new Vector(numOfEquations);
                for (sbyte j = 0; j < i; j++)
                    temp += method.a[i - 1][j] * k[j];
                k[i] = f(t + method.c[i - 1] * h, y + h * temp, parameter);
            }
            return k;
        }
        //вычисляет приращение для y и y с крышкой
        protected void SetChanges(Method method, out Vector yChange, out Vector y_Change, double h, Vector y, double t, double parameter)
        {
            yChange = new Vector(numOfEquations);
            y_Change = new Vector(numOfEquations);
            Vector[] k = GetK(method, h, y, t, parameter); //массив векторов k1, k2, ..., ks, в нашем случае s = numOfSteps
            for (sbyte i = 0; i < method.numOfSteps; i++)
            {
                yChange += method.y1[i] * k[i];
                y_Change += method.y1_[i] * k[i];
            }
            yChange *= h;
            y_Change *= h;
        }
        protected double GetHNew(Method method, double errLocal, double eps, double h)
        {
            double kappa = Math.Pow(errLocal / eps, 1.0 / method.methodOrder);
            if (kappa > 6.0) kappa = 6.0;
            if (kappa < 0.1) kappa = 0.1;
            return 0.9 * h / kappa;
        }

        public Problem(sbyte numOfEquations, Func<double, Vector, double, Vector> f, Func<double, Vector, double> Lambda)
        {
            if (numOfEquations < 1) throw new ProblemException("Incorrect number of equations.");

            this.numOfEquations = numOfEquations;
            this.f = f;
            this.Lambda = Lambda;
        }
    }
    class CauchyProblem : Problem
    {
        public readonly Conditions conditions;
        public readonly double tLast;

        public Results Solve(Method method, sbyte numOfPoints, double eps, double parameter)
        {
            Points points = new Points(conditions.t0, tLast, numOfPoints);
            return Solve(method, points, conditions, eps, parameter);
        }

        public CauchyProblem(Conditions conditions, double tLast, sbyte numOfEquations,
            Func<double, Vector, double, Vector> f, Func<double, Vector, double> Lambda) : base(numOfEquations, f, Lambda)
        {
            this.conditions = conditions;
            this.tLast = tLast;
        }
    }
    class CauchyProblemWithFixedParameter : CauchyProblem
    {
        public readonly double parameter;

        public Results Solve(Method method, sbyte numOfPoints, double eps)
        {
            return Solve(method, numOfPoints, eps, parameter);
        }

        public CauchyProblemWithFixedParameter(double parameter,
            Conditions conditions, double tLast, sbyte numOfEquations, Func<double, Vector, double, Vector> f,
                Func<double, Vector, double> Lambda) : base(conditions, tLast, numOfEquations, f, Lambda)
        {
            this.parameter = parameter;
        }
    }
    class LagrangeProblem : Problem
    {
        readonly Func<Vector, Conditions> BuildConditions;
        readonly Func<Vector, Vector> ExtractComponents;
        readonly double tLast;
        
        readonly SystemOfNonLinearEquations systemOfNonLinearEquations;

        Vector F(Vector x, double epsilon, double parameter, Method method)
        {
            Conditions conditions = BuildConditions(x);
            return ExtractComponents(Solve(method, tLast, conditions, epsilon, parameter).y);
        }

        public CauchyProblemWithFixedParameter ConvertToCauchyProblem(double epsilon, double parameter, Method method)
        {
            Vector foundComponentsOfConditions =
                systemOfNonLinearEquations.ApplyParameterContinuationMethod(epsilon, parameter, method);
            Conditions conditions = BuildConditions(foundComponentsOfConditions);
            return new CauchyProblemWithFixedParameter(parameter, conditions, tLast, numOfEquations, f, Lambda);
        }

        public LagrangeProblem(Func<Vector, Conditions> BuildConditions, Func<Vector, Vector> ExtractComponents,
            double tLast, double initialParameter, Vector analyticalSolutionForInitialParameter,
            sbyte numOfEquations, Func<double, Vector, double, Vector> f, Func<double, Vector, double> Lambda) : base(numOfEquations, f, Lambda)
        {
            this.BuildConditions = BuildConditions;
            this.ExtractComponents = ExtractComponents;
            this.tLast = tLast;
            systemOfNonLinearEquations = new SystemOfNonLinearEquations(analyticalSolutionForInitialParameter, initialParameter, F);
        }
    }
    class UnknownPointProblem : Problem //система уравнений с заданным значением в конечной точке (но сама точка не задана)
    {
        public readonly Conditions conditions;
        //специальная корректировка шага, определяемая конкретной задачей
        public readonly Func<Vector, Vector, double, double, double> AdjustStep;
        //проверка на достижение нужной точки
        public readonly Func<Vector, double, bool> IsPointReached;

        public CauchyProblem ConvertToCauchyProblem(Method method, double eps, double parameter)
        {
            double tLast = GetPoint(method, eps, parameter);
            return new CauchyProblem(conditions, tLast, numOfEquations, f, Lambda);
        }
        public double GetPoint(Method method, double eps, double parameter)
        {
            double h = 1;
            double hAdjusted;
            Vector yChange; //соответствует "дельта игрик", содержит приращение функции y
            Vector y_Change; //приращение для "игрик с крышкой"
            Vector y = conditions.y0; //начальное значение функции y берется в точке t = tMin
            double errLocal;
            double t = conditions.t0;
            while (true) //каждую итерацию корректируем шаг, и если шаг хороший, шагаем
            {
                SetChanges(method, out yChange, out y_Change, h, y, t, parameter); //получаем приращения для y и y с крышкой
                errLocal = (yChange - y_Change).Length;
                if (errLocal < eps)
                {
                    //корректируем шаг если надо
                    hAdjusted = AdjustStep(y, yChange, eps, h);

                    //если шаг скорректирован
                    if (hAdjusted != h)
                    {
                        h = hAdjusted;
                        continue;
                    }
                    t += h; //переходим к следующей точке
                    y += yChange; //получаем значение в следующей точке
                    //Console.WriteLine("y({0}) = {1}", t, y);
                    //Thread.Sleep(100);
                    //если мы достигли нужной точки, выходим
                    if (IsPointReached(y, eps)) return t;
                }
                h = GetHNew(method, errLocal, eps, h);
            }
        }

        public UnknownPointProblem(Conditions conditions, sbyte numOfEquations,
            Func<double, Vector, double, Vector> f, Func<double, Vector, double> Lambda, Func<Vector, Vector, double,
                double, double> AdjustStep, Func<Vector, double, bool> IsPointReached) : base(numOfEquations, f, Lambda)
        {
            this.conditions = conditions;
            this.AdjustStep = AdjustStep;
            this.IsPointReached = IsPointReached;
        }
    }

    class ProblemException : Exception
    {
        public ProblemException() : base("Incorrect problem.") { }
        public ProblemException(string message) : base(message) { }
    }
}
