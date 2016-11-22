using System;

namespace LagrangeProblem
{
    class MyProblem3
    {
        static readonly double parameter = 0.0;
        static Vector f(double t, Vector y, double parameter)
        {
            double y0 = y[1];
            double y1 = y[3] - y[0] * Math.Pow(Math.E, -parameter * y[0]);
            double y2 = y[3] * Math.Pow(Math.E, -parameter * y[0]) * (1 - parameter * y[0]);
            double y3 = -y[2];

            return new Vector(y0, y1, y2, y3);
        }
        static double Lambda(double t, Vector y, double parameter)
        {
            double a = -Math.Pow(Math.E, -parameter * y[0]) * (1 - parameter * y[0]);
            double b = parameter * y[3] * Math.Pow(Math.E, -parameter * y[0]) * (-2 + parameter * y[0]);
            double c = Math.Abs(b - 1) * Math.Sqrt((a + 1) * (a + 1) + (b + 1) * (b + 1) / 4);
            return Math.Sqrt(c + (a + 1) * (a + 1) + b * b / 2 + 0.5) / 2;
        }
        //строит полные начальные условия из значений, полученных для неизвестных начальных условий
        static Conditions BuildConditions(Vector components)
        {
            return new Conditions(0.0, new Vector(0.0, components[0], components[1], 0.0));
        }
        //извлекает известные конечные условия
        static Vector ExtractComponents(Vector y)
        {
            return new Vector(y[0], y[1] + (Math.PI / 2));
        }

        static readonly sbyte numOfEquations = 4;
        static readonly string fileName = "../../Felberg.txt";
        static readonly double tLast = Math.PI / 2;
        static readonly double epsilon1 = 1e-7;
        static readonly double epsilon2 = 1e-9;
        static readonly double epsilon3 = 1e-11;
        static readonly double initialParameter = 0.0; //параметр, при котором известно аналитическое решение
        static readonly Vector analyticalSolutionForInitialParameter = new Vector(1, 2);
        static readonly sbyte requiredNumOfPoints = 4;

        public static void Solve()
        {
            //Создаем экземпляр задачи Лагранжа
            LagrangeProblem lagrangeProblem = new LagrangeProblem(BuildConditions, ExtractComponents,
                tLast, initialParameter, analyticalSolutionForInitialParameter, numOfEquations, f, Lambda);

            //создаем поставщик данных метода
            IMethodProvider provider = new FileMethodProvider(fileName);

            //создаем метод из данных, полученных от поставщика
            Method method = new Method(provider);

            //найдем начальные условия в задаче Лагранжа, и составим из них задачу Коши
            CauchyProblemWithFixedParameter cauchyProblem =
                lagrangeProblem.ConvertToCauchyProblem(epsilon3, parameter, method);

            //решаем полученную задачу с разной степенью точности
            Results results1 = cauchyProblem.Solve(method, requiredNumOfPoints, epsilon1);
            Results results2 = cauchyProblem.Solve(method, requiredNumOfPoints, epsilon2);
            Results results3 = cauchyProblem.Solve(method, requiredNumOfPoints, epsilon3);

            //создаем визуализатор результатов в консоль
            ResultsRenderer renderer = new ConsoleRenderer();
            ResultsRenderer rendererTex1 = new LaTeXRenderer("table1.tex");
            ResultsRenderer rendererTex2 = new LaTeXRenderer("table2.tex");
            ResultsRenderer rendererTex3 = new LaTeXRenderer("table3.tex");
            ResultsRenderer rendererTex4 = new LaTeXRenderer("table4.tex");

            //выводим результаты в консоль
            renderer.RenderResults(results1, "Таблица 1.");
            renderer.RenderResults(results2, "Таблица 2.");
            renderer.RenderResults(results3, "Таблица 3.");

            //выводим результаты в tex-файлы
            rendererTex1.RenderResults(results1, "Таблица 1.");
            rendererTex2.RenderResults(results2, "Таблица 2.");
            rendererTex3.RenderResults(results3, "Таблица 3.");

            //выводим соотношения результатов
            renderer.RenderResultsRelation(results1, results2, results3, "Таблица 4.");
            rendererTex4.RenderResultsRelation(results1, results2, results3, "Таблица 4.");

            //выводим найденные начальные условия
            Console.WriteLine();
            Console.WriteLine("Начальные условия для параметра alpha = {0}:", parameter);
            Console.WriteLine(cauchyProblem.conditions.y0);

            //Всё!
        }
    }
}
