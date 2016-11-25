using System;
using System.Collections.Generic;

namespace LagrangeProblem
{
    class Problem //инкапсулирует систему уравнений с начальными условиями
    {
        public readonly sbyte numOfEquations; //число дифференциальных уравнений первого порядка в системе
        //векторная функция, стоящая в правой части системы уравнений
        protected readonly Func<double, Vector, double, Vector> f;
        //функция от t, используюящаяся в вычислении глобальной погрешности
        protected readonly Func<double, Vector, double, double> Lambda;

        //решает в нескольких точках
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
        //решает только в одной точке
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
            int i = 0;
            while (t < point - eps) //каждую итерацию корректируем шаг, и если шаг хороший, шагаем
            {
                if (t + h > point) h = point - t; //чтобы случайно не перешагнуть следующюю точку
                SetChanges(method, out yChange, out y_Change, h, y, t, parameter); //получаем приращения для y и y с крышкой
                errLocal = (yChange - y_Change).Length;
                if (errLocal < eps)
                {
                    lambda = Lambda(t, y, parameter); //вычисляем её именно здесь, так как нужно значение в предыдущей точке
                    t += h; //переходим к следующей точке
                    y += yChange; //получаем значение в следующей точке
                    errGlobal = errLocal + errGlobal * Math.Pow(Math.E, lambda * h);
                }
                h = GetHNew(method, errLocal, eps, h);
                i++;
                //если не удается решить задачу Коши за чрезчур большое число итераций
                if(i > 15000)
                {
                    throw new ProblemException("Cauchy problem can't be solved.");
                }
            }
            return y;
        }
        //возвращает числа k1, k2, ..., ks для явного метода РК
        Vector[] GetK(Method method, double h, Vector y, double t, double parameter)
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
        protected void SetChanges(Method method, out Vector yChange,
            out Vector y_Change, double h, Vector y, double t, double parameter)
        {
            yChange = new Vector(numOfEquations);
            y_Change = new Vector(numOfEquations);
            //массив векторов k1, k2, ..., ks, в нашем случае s = numOfSteps
            Vector[] k = GetK(method, h, y, t, parameter);
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

        public Problem(sbyte numOfEquations,
            Func<double, Vector, double, Vector> f,Func<double, Vector, double, double> Lambda)
        {
            if (numOfEquations < 1) throw new ProblemException("Incorrect number of equations.");

            this.numOfEquations = numOfEquations;
            this.f = f;
            this.Lambda = Lambda;
        }
    }
}
