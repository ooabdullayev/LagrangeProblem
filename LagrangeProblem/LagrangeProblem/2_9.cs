using System;
using System.IO;
using System.Globalization;

namespace LagrangeProblem
{
    class _2_9
    {
        static double alpha = 0.5;
        static Vector f(double t, Vector y)
        {
            double y0 = y[1];
            double y1 = Math.Sin(t) - alpha * Math.Sin(y[0]);

            return new Vector(y0, y1);
        }
        static double Lambda(double t, Vector y)
        {
            return (1 - alpha * Math.Cos(y[0])) / 2;
        }
        public static void Solve(sbyte numOfPoints, string fileName)
        {
            sbyte numOfEquations = 2;
            double t0 = 0;
            double tLast = Math.PI / 2;
            Vector y0 = new Vector(0, 1);
            double epsilon1 = 1e-7;
            double epsilon2 = 1e-9;
            double epsilon3 = 1e-11;
            Conditions conditions = new Conditions(t0, y0);

            //создаем классическую задачу Коши
            ClassicProblem myProblem = new ClassicProblem(conditions, tLast, numOfEquations, f, Lambda);

            //создаем поставщик метода
            IMethodProvider provider = new FileMethodProvider(fileName);

            //создаем метод
            Method method = new Method(provider);

            //получаем результаты решения нашего уравнения для трёх разных допустимых погрешностей
            Results results1 = myProblem.Solve(method, numOfPoints, epsilon1);
            Results results2 = myProblem.Solve(method, numOfPoints, epsilon2);
            Results results3 = myProblem.Solve(method, numOfPoints, epsilon3);
            
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
        public static void GetDataForGraphics(string methodFileName, string outputFileName, double step)
        {
            double initialAlpha = alpha;
            sbyte numOfPoints = 1;
            sbyte numOfEquations = 2;
            double t0 = 0;
            double tLast = Math.PI / 2;
            Vector y0 = new Vector(0, 1);
            double epsilon = 1e-11;
            Conditions conditions = new Conditions(t0, y0);
            StreamWriter outputFile = new StreamWriter(outputFileName);

            //создаем классическую задачу Коши
            ClassicProblem myProblem = new ClassicProblem(conditions, tLast, numOfEquations, f, Lambda);

            //создаем поставщик метода
            IMethodProvider provider = new FileMethodProvider(methodFileName);

            //создаем метод
            Method method = new Method(provider);
            
            Result result;

            for(alpha = step; alpha < 1; alpha += step)
            {
                result = myProblem.Solve(method, numOfPoints, epsilon)[0];
                outputFile.WriteLine(alpha.ToString("G", CultureInfo.InvariantCulture) + "\t" + result.y[1].ToString("G", CultureInfo.InvariantCulture));
            }
            outputFile.Close();
            alpha = initialAlpha;
        }
    }
}
