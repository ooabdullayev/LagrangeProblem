using System;
//разработать неизменяемый массив

//нужно поместить файл с именем метода, содержащий таблицу метода в директорию с приложением и запустить
//в файле может содержаться таблица любого другого s-стадийного ЯМРК вместе с числом стадий и порядка
//при желании можно изменить условия системы уравнений, но система должна быть первого порядка,
//все начальные условия должны быть заданы

namespace LagrangeProblem
{
    class Program
    {
        static void Main()
        {
            //_2_18.Solve();
            //_2_19.Solve();
            //_2_2.Solve(0.1, 4, "Felberg.txt", "DormanPrince.txt");
            //_2_4.Solve(4, "DormanPrince.txt");
            //_2_9.Solve(4, "Felberg.txt");
            //_2_9.GetDataForGraphics("Felberg.txt", "data.log", 1.0 / 100.0);
            //_2_15.Solve(4, "Felberg.txt");
            MyProblem3.Solve();

            Console.ReadKey();
        }
    }
}
