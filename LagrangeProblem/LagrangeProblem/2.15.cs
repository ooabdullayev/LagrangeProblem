using System;

namespace LagrangeProblem
{
    class _2_15
    {
        static double parameter = 0.0;
        static double alpha = 1.1;
        static double beta = 5.0;
        static Vector f(double t, Vector y, double parameter)
        {
            double y0 = y[1];
            double y1 = y[0] * (1 + t * t) / alpha;

            return new Vector(y0, y1);
        }
        static double Lambda(double t, Vector y)
        {
            return (t * t + alpha + 1) / (2 * alpha);
        }
        public static void Solve(sbyte numOfPoints, string fileName)
        {
            sbyte numOfEquations = 2;
            double t0 = 0;
            double tLast = beta;
            Vector y0 = new Vector(1, 0);
            double epsilon1 = 1e-7;
            double epsilon2 = 1e-9;
            double epsilon3 = 1e-11;
            Conditions conditions = new Conditions(t0, y0);

            //создаем классическую задачу Коши
            CauchyProblem myProblem = new CauchyProblem(conditions, tLast, numOfEquations, f, Lambda);

            //создаем поставщик метода
            IMethodProvider provider = new FileMethodProvider(fileName);

            //создаем метод
            Method method = new Method(provider);

            //получаем результаты решения нашего уравнения для трёх разных допустимых погрешностей
            Results results1 = myProblem.Solve(method, numOfPoints, epsilon1, parameter);
            Results results2 = myProblem.Solve(method, numOfPoints, epsilon2, parameter);
            Results results3 = myProblem.Solve(method, numOfPoints, epsilon3, parameter);
            
            //создаем места вывода наших результатов
            ResultsRenderer laTeXRenderer1 = new LaTeXRenderer("tableEps1.tex");
            ResultsRenderer laTeXRenderer2 = new LaTeXRenderer("tableEps2.tex");
            ResultsRenderer laTeXRenderer3 = new LaTeXRenderer("tableEps3.tex");
            ResultsRenderer laTeXRendererRelation = new LaTeXRenderer("tableRelation.tex");

            ResultsRenderer consoleRenderer = new ConsoleRenderer();

            //выводим резултаты
            laTeXRenderer1.RenderResults(results1, "Таблица 1");
            laTeXRenderer2.RenderResults(results2, "Таблица 2");
            laTeXRenderer3.RenderResults(results3, "Таблица 3");
            laTeXRendererRelation.RenderResultsRelation(results1, results2, results3, "Таблица 4");

            Console.WriteLine();
            Console.WriteLine("By Felberg method.");

            consoleRenderer.RenderResults(results1, "Таблица 1");
            consoleRenderer.RenderResults(results2, "Таблица 2");
            consoleRenderer.RenderResults(results3, "Таблица 3");
            consoleRenderer.RenderResultsRelation(results1, results2, results3, "Таблица 4");
        }
    }
}
