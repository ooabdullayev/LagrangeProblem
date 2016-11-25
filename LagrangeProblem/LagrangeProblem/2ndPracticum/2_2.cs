using System;

namespace LagrangeProblem
{
    class _2_2
    {
        static readonly double parameter = 0.1;
        static Vector f(double t, Vector y, double parameter)
        {
            double y0 = y[1];
            double y1 = 2 * y[0] * (2 * y[0] * y[0] - 1);
            y1 *= 1 + y[0] * y[0] - y[0] * y[0] * y[0] * y[0] - y[2];
            double y2 = y[3];
            double y3 = 1 + y[0] * y[0] - y[0] * y[0] * y[0] * y[0] - y[2];

            return new Vector(y0, y1, y2, y3);
        }
        static double Lambda(double t, Vector y, double parameter)
        {
            double u, v;
            double result;

            u = 2 * (6 * y[0] * y[0] - 1) * (1 + y[0] * y[0] - y[0] * y[0] * y[0] * y[0] - y[2]);
            u -= 4 * y[0] * y[0] * (2 * y[0] * y[0] - 1) * (2 * y[0] * y[0] - 1);
            u = (u + 1) / 2;

            v = y[0] * (1 - 2 * y[0] * y[0]);

            result = u * u + 2 * v * v;
            result += Math.Sqrt(result * result - 4 * v * v * v * v);
            result /= 2;
            return Math.Sqrt(result);

        }

        static readonly sbyte numOfEquations = 4;
        static readonly string fileName1 = "../../Felberg.txt";
        static readonly string fileName2 = "../../DormanPrince.txt";
        static readonly double tLast = 50.0;
        static readonly double epsilon1 = 1e-7;
        static readonly double epsilon2 = 1e-9;
        static readonly double epsilon3 = 1e-11;
        static readonly Conditions conditions =
            new Conditions(0.0, new Vector(parameter, 0.0, parameter * parameter * (1 - parameter * parameter), 0.0));
        static readonly sbyte requiredNumberOfPoints = 4;

        public static void Solve()
        {
            //создаем классическую задачу Коши
            CauchyProblem myProblem = new CauchyProblem(conditions, tLast, numOfEquations, f, Lambda);

            //создаем поставщиков разных методов
            IMethodProvider provider1 = new FileMethodProvider(fileName1);
            IMethodProvider provider2 = new FileMethodProvider(fileName2);

            //создаем методы
            Method method1 = new Method(provider1);
            Method method2 = new Method(provider2);

            //получаем результаты решения нашего уравнения для трёх разных допустимых погрешностей
            Results results11 = myProblem.Solve(method1, requiredNumberOfPoints, epsilon1, parameter);
            Results results12 = myProblem.Solve(method1, requiredNumberOfPoints, epsilon2, parameter);
            Results results13 = myProblem.Solve(method1, requiredNumberOfPoints, epsilon3, parameter);

            Results results21 = myProblem.Solve(method2, requiredNumberOfPoints, epsilon1, parameter);
            Results results22 = myProblem.Solve(method2, requiredNumberOfPoints, epsilon2, parameter);
            Results results23 = myProblem.Solve(method2, requiredNumberOfPoints, epsilon3, parameter);

            //создаем места вывода наших результатов
            ResultsRenderer laTeXRenderer1 = new LaTeXRenderer("tableEps1.tex");
            ResultsRenderer laTeXRenderer2 = new LaTeXRenderer("tableEps2.tex");
            ResultsRenderer laTeXRenderer3 = new LaTeXRenderer("tableEps3.tex");
            ResultsRenderer laTeXRendererRelation = new LaTeXRenderer("tableRelation.tex");

            ResultsRenderer consoleRenderer = new ConsoleRenderer();

            //выводим резултаты
            laTeXRenderer1.RenderResults(results11, "Таблица 1");
            laTeXRenderer2.RenderResults(results12, "Таблица 2");
            laTeXRenderer3.RenderResults(results13, "Таблица 3");
            laTeXRendererRelation.RenderResultsRelation(results11, results12, results13, "Таблица 4");

            Console.WriteLine();
            Console.WriteLine("By Felberg method.");

            consoleRenderer.RenderResults(results11, "Таблица 1");
            consoleRenderer.RenderResults(results12, "Таблица 2");
            consoleRenderer.RenderResults(results13, "Таблица 3");
            consoleRenderer.RenderResultsRelation(results11, results12, results13, "Таблица 4");

            Console.WriteLine();
            Console.WriteLine("By Dorman - Prince method.");

            consoleRenderer.RenderResults(results21, "Таблица 1");
            consoleRenderer.RenderResults(results22, "Таблица 2");
            consoleRenderer.RenderResults(results23, "Таблица 3");
            consoleRenderer.RenderResultsRelation(results21, results22, results23, "Таблица 4");
        }
    }
}
