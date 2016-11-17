using System;

namespace LagrangeProblem
{
    class _2_18
    {
        static Vector f(double t, Vector y)
        {
            double y0 = y[1];
            double y1 = -Math.Cos(y[0]) + Math.Sin(t);

            return new Vector(y0, y1);
        }
        static double Lambda(double t, Vector y)
        {
            return (Math.Sin(y[0]) + 1) / 2;
        }
        static Conditions MakeConditions(double alpha)
        {
            return new Conditions(0, new Vector(alpha, 0.0));
        }
        static double GetCombinationOfComponents(Vector y)
        {
            return y[0];
        }

        static readonly sbyte numOfEquations = 2;
        static readonly string fileName = "../../Felberg.txt";
        static readonly double tLast = Math.PI / 2;
        static readonly double epsilon1 = 1e-7;
        static readonly double epsilon2 = 1e-9;
        static readonly double epsilon3 = 1e-11;
        static readonly double previousStartingPoint = 0.2;
        static readonly double nextStartingPoint = 0.5;
        static readonly sbyte requiredNumOfPoints = 4;

        //Создаем экземпляр задачи
        static readonly Problem problem = new Problem(numOfEquations, f, Lambda);
        //создаем поставщик данных метода
        static readonly IMethodProvider provider = new FileMethodProvider(fileName);
        //создаем метод из данных, полученных от поставщика
        static readonly Method method = new Method(provider);
        //задаем функцию для нелинейного уравнения с одной неизвестной
        static double F(double x)
        {
            Conditions conditions = MakeConditions(x);
            return GetCombinationOfComponents(problem.Solve(method, tLast, conditions, epsilon3).y);
        }

        public static void Solve()
        {
            //создаем нелинейное уравнение с одной неизвестной
            NonLinearEquation nonLinEquation = new NonLinearEquation(previousStartingPoint, nextStartingPoint, F);

            //решаем уравнение методом хорд и из корня составляем полные начальные условия для задачи Коши
            Conditions foundConditions = MakeConditions(nonLinEquation.ApplyMethodOfChords(epsilon3));

            //создаем экземпляр классической задачи Коши из с уже известными начальными условиями
            ClassicProblem clProblem = new ClassicProblem(foundConditions, tLast, numOfEquations, f, Lambda);

            //решаем полученную задачу с разной степенью точности
            Results results1 = clProblem.Solve(method, requiredNumOfPoints, epsilon1);
            Results results2 = clProblem.Solve(method, requiredNumOfPoints, epsilon2);
            Results results3 = clProblem.Solve(method, requiredNumOfPoints, epsilon3);

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
