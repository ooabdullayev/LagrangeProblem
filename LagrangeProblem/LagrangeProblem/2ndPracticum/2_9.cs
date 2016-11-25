using System;
using System.IO;
using System.Globalization;

namespace LagrangeProblem
{
    class _2_9
    {
        static readonly double parameter = 0.5;
        static Vector f(double t, Vector y, double parameter)
        {
            double y0 = y[1];
            double y1 = Math.Sin(t) - parameter * Math.Sin(y[0]);

            return new Vector(y0, y1);
        }
        static double Lambda(double t, Vector y, double parameter)
        {
            return (1 - parameter * Math.Cos(y[0])) / 2;
        }

        static readonly sbyte numOfEquations = 2;
        static readonly string fileName = "../../Felberg.txt";
        static readonly Conditions conditions = new Conditions(0, new Vector(0, 1));
        static readonly double tLast = Math.PI / 2;
        static readonly double epsilon1 = 1e-7;
        static readonly double epsilon2 = 1e-9;
        static readonly double epsilon3 = 1e-11;
        static readonly sbyte requiredNumOfPoints = 4;

        static readonly string dataFileName = "data.log";
        static readonly double stepForGraphicsData = 1.0 / 100.0;

        public static void Solve()
        {
            //выводим данные для построения графика в файл
            GetDataForGraphics(dataFileName, stepForGraphicsData, epsilon3);

            //создаем классическую задачу Коши
            CauchyProblem myProblem = new CauchyProblem(conditions, tLast, numOfEquations, f, Lambda);

            //создаем поставщик метода
            IMethodProvider provider = new FileMethodProvider(fileName);

            //создаем метод
            Method method = new Method(provider);

            //получаем результаты решения нашего уравнения для трёх разных допустимых погрешностей
            Results results1 = myProblem.Solve(method, requiredNumOfPoints, epsilon1, parameter);
            Results results2 = myProblem.Solve(method, requiredNumOfPoints, epsilon2, parameter);
            Results results3 = myProblem.Solve(method, requiredNumOfPoints, epsilon3, parameter);

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
        static void GetDataForGraphics(string outputFileName, double step, double epsilon)
        {
            sbyte numOfPoints = 1;
            StreamWriter outputFile = new StreamWriter(outputFileName);

            //создаем классическую задачу Коши
            CauchyProblem myProblem = new CauchyProblem(conditions, tLast, numOfEquations, f, Lambda);

            //создаем поставщик метода
            IMethodProvider provider = new FileMethodProvider(fileName);

            //создаем метод
            Method method = new Method(provider);

            Result result;

            for (double parameter = step; parameter < 1; parameter += step)
            {
                result = myProblem.Solve(method, numOfPoints, epsilon, parameter)[0];
                outputFile.WriteLine(parameter.ToString("G",
                    CultureInfo.InvariantCulture) + "\t" + result.y[1].ToString("G", CultureInfo.InvariantCulture));
            }
            outputFile.Close();
        }
    }
}
