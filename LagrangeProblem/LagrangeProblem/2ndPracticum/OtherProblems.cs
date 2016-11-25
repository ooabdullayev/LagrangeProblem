using System;
using System.Collections.Generic;

namespace LagrangeProblem
{
    class UnknownCondProblem : Problem
    {
        //строит полные начальные условия из значений, полученных для неизвестных начальных условий
        readonly Func<double, Conditions> BuildConditions;
        //извлекает известные конечные условия
        readonly Func<Vector, double> ExtractComponents;
        public readonly double tLast;

        readonly OneNonLinearEquation nonLinearEquation;

        double F(double x, double epsilon, double parameter, Method method)
        {
            Conditions conditions = BuildConditions(x);
            return ExtractComponents(Solve(method, tLast, conditions, epsilon, parameter).y);
        }

        //метод, для получения из задачи с неполными начальными условиями
        //задачи Коши с помощью вычисления начальных условий
        public CauchyProblemWithFixedParameter ConvertToCauchyProblem(double epsilon, double parameter, Method method)
        {
            //находим неизвестное начальное условие
            double foundComponentOfConditions =
                nonLinearEquation.ApplyMethodOfChords(epsilon, parameter, method);
            //составляем из него полное начальное условие
            Conditions conditions = BuildConditions(foundComponentOfConditions);
            return new CauchyProblemWithFixedParameter(parameter, conditions, tLast, numOfEquations, f, Lambda);
        }

        public UnknownCondProblem(Func<double, Conditions> BuildConditions,
            Func<Vector, double> ExtractComponents, double tLast, double previousStartingPoint,
                double nextStartingPoint, sbyte numOfEquations, Func<double, Vector, double, Vector> f,
                    Func<double, Vector, double, double> Lambda) : base(numOfEquations, f, Lambda)
        {
            this.BuildConditions = BuildConditions;
            this.ExtractComponents = ExtractComponents;
            this.tLast = tLast;
            nonLinearEquation = new OneNonLinearEquation(previousStartingPoint, nextStartingPoint, F);
        }
    }
    //система уравнений с заданным значением в конечной точке (но сама точка не задана)
    class UnknownPointProblem : Problem
    {
        public readonly Conditions conditions;
        //специальная корректировка шага, определяемая конкретной задачей
        readonly Func<Vector, Vector, double, double, double> AdjustStep;
        //проверка на достижение нужной точки
        readonly Func<Vector, double, bool> IsPointReached;

        public CauchyProblem ConvertToCauchyProblem(Method method, double eps, double parameter)
        {
            double tLast = GetPoint(method, eps, parameter);
            return new CauchyProblem(conditions, tLast, numOfEquations, f, Lambda);
        }
        double GetPoint(Method method, double eps, double parameter)
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
            Func<double, Vector, double, Vector> f, Func<double, Vector, double, double> Lambda, Func<Vector, Vector, double,
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
