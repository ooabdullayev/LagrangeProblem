using System;

namespace LagrangeProblem
{
    class _2_19
    {
        static readonly double parameter = 0.0; //заглушка
        static Vector f(double t, Vector y, double parameter)
        {
            double y0 = y[1];
            double y1 = -0.5 * Math.Sin(y[0]) + Math.Sin(t);

            return new Vector(y0, y1);
        }
        static double Lambda(double t, Vector y, double parameter)
        {
            return 0.5 - 0.25 * Math.Cos(y[0]);
        }
        //строит полные начальные условия из значений, полученных для неизвестных начальных условий
        static Conditions BuildConditions(double component)
        {
            return new Conditions(0, new Vector(0.0, component));
        }
        //извлекает известные конечные условия
        static double ExtractComponents(Vector y)
        {
            return y[1];
        }

        static readonly sbyte numOfEquations = 2;
        static readonly string fileName = "../../Felberg.txt";
        static readonly double tLast = Math.PI / 2;
        static readonly double epsilon1 = 1e-7;
        static readonly double epsilon2 = 1e-9;
        static readonly double epsilon3 = 1e-11;
        static readonly double previousStartingPoint = -2.0;
        static readonly double nextStartingPoint = -1.0;
        static readonly sbyte requiredNumOfPoints = 4;

        public static void Solve()
        {
            //Создаем экземпляр задачи
            UnknownCondProblem problem =
                new UnknownCondProblem(BuildConditions, ExtractComponents, tLast,
                    previousStartingPoint, nextStartingPoint, numOfEquations, f, Lambda);

            //создаем поставщик данных метода
            IMethodProvider provider = new FileMethodProvider(fileName);

            //создаем метод из данных, полученных от поставщика
            Method method = new Method(provider);

            //найдем начальные условия в задаче, и составим из них задачу Коши
            CauchyProblemWithFixedParameter cauchyProblem =
                problem.ConvertToCauchyProblem(epsilon3, parameter, method);

            //решаем полученную задачу с разной степенью точности
            Results results1 = cauchyProblem.Solve(method, requiredNumOfPoints, epsilon1, parameter);
            Results results2 = cauchyProblem.Solve(method, requiredNumOfPoints, epsilon2, parameter);
            Results results3 = cauchyProblem.Solve(method, requiredNumOfPoints, epsilon3, parameter);

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

            rendererTex1.RenderResults(results1, "Таблица 1.");
            rendererTex2.RenderResults(results2, "Таблица 2.");
            rendererTex3.RenderResults(results3, "Таблица 3.");

            //выводим соотношения результатов
            renderer.RenderResultsRelation(results1, results2, results3, "Таблица 4.");
            rendererTex4.RenderResultsRelation(results1, results2, results3, "Таблица 4.");

            //Всё!
        }
    }
}
