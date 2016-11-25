using System;
using System.Collections.Generic;

namespace LagrangeProblem
{
    class CauchyProblem : Problem
    {
        public readonly Conditions conditions; //начальные условия
        public readonly double tLast;

        public Results Solve(Method method, sbyte numOfPoints, double eps, double parameter)
        {
            Points points = new Points(conditions.t0, tLast, numOfPoints);
            return Solve(method, points, conditions, eps, parameter);
        }

        public CauchyProblem(Conditions conditions, double tLast,
            sbyte numOfEquations, Func<double, Vector, double, Vector> f,
                Func<double, Vector, double, double> Lambda) : base(numOfEquations, f, Lambda)
        {
            this.conditions = conditions;
            this.tLast = tLast;
        }
    }
    //класс, решающий задачу для фиксированного параметра
    //необходим, чтобы когда мы получили начальные условия для задачи Коши для с заданным параметром, мы по ошибке
    //не решали данную задачу с другим параметром
    class CauchyProblemWithFixedParameter : CauchyProblem
    {
        public readonly double parameter;

        public Results Solve(Method method, sbyte numOfPoints, double eps)
        {
            return Solve(method, numOfPoints, eps, parameter);
        }

        public CauchyProblemWithFixedParameter(double parameter,
            Conditions conditions, double tLast, sbyte numOfEquations, Func<double, Vector, double, Vector> f,
                Func<double, Vector, double, double> Lambda) : base(conditions, tLast, numOfEquations, f, Lambda)
        {
            this.parameter = parameter;
        }
    }
    class LagrangeProblem : Problem
    {
        //строит полные начальные условия из значений, полученных для неизвестных начальных условий
        readonly Func<Vector, Conditions> BuildConditions;
        //извлекает известные конечные условия
        readonly Func<Vector, Vector> ExtractComponents;
        public readonly double tLast;

        readonly SystemOfNonLinearEquations systemOfNonLinearEquations;

        Vector F(Vector x, double epsilon, double parameter, Method method)
        {
            Conditions conditions = BuildConditions(x);
            return ExtractComponents(Solve(method, tLast, conditions, epsilon, parameter).y);
        }

        //метод, для получения из задачи Лагранжа задачи Коши с помощью вычисления начальных условий
        public CauchyProblemWithFixedParameter ConvertToCauchyProblem(double epsilon, double parameter, Method method)
        {
            //находим неизвестные начальные условия
            Vector foundComponentsOfConditions =
                systemOfNonLinearEquations.ApplyParameterContinuationMethod(epsilon, parameter, method);
            //составляем из них полные начальные условия
            Conditions conditions = BuildConditions(foundComponentsOfConditions);
            return new CauchyProblemWithFixedParameter(parameter, conditions, tLast, numOfEquations, f, Lambda);
        }

        public LagrangeProblem(Func<Vector, Conditions> BuildConditions,
            Func<Vector, Vector> ExtractComponents, double tLast, double initialParameter,
                Vector analyticalSolutionForInitialParameter, sbyte numOfEquations, Func<double, Vector,
                    double, Vector> f, Func<double, Vector, double, double> Lambda) : base(numOfEquations, f, Lambda)
        {
            this.BuildConditions = BuildConditions;
            this.ExtractComponents = ExtractComponents;
            this.tLast = tLast;
            systemOfNonLinearEquations =
                new SystemOfNonLinearEquations(analyticalSolutionForInitialParameter, initialParameter, F);
        }
    }
}
