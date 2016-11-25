using System;

namespace LagrangeProblem
{
    class OneNonLinearEquation //инкапсулирует нелинейное уравнение и содержит метод хорд
    {
        //данные начальные точки для метода хорд
        readonly double previousStartingPoint;
        readonly double nextStartingPoint;

        //сама нелинейная функция
        readonly Func<double, double, double, Method, double> F;

        public double ApplyMethodOfChords(double epsilon, double parameter, Method method)
        {
            double previousPoint = previousStartingPoint;
            double nextPoint = nextStartingPoint;
            double nextValue = F(nextPoint, epsilon, parameter, method);
            double previousValue = F(previousPoint, epsilon, parameter, method);

            while (Math.Abs(nextValue) >= epsilon)
            {
                nextPoint = nextPoint - nextValue * (nextPoint - previousPoint) / (nextValue - previousValue);
                nextValue = F(nextPoint, epsilon, parameter, method);
            }
            return nextValue;
        }

        public OneNonLinearEquation(double previousStartingPoint,
            double nextStartingPoint, Func<double, double, double, Method, double> F)
        {
            this.previousStartingPoint = previousStartingPoint;
            this.nextStartingPoint = nextStartingPoint;
            this.F = F;
        }
    }
}