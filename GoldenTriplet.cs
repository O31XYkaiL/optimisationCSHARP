using System.Globalization;
using static System.Math;

namespace GoldenTriplet
{
    public class lr3
    {
        private readonly Func<double, double> func;
        private readonly double start;
        private readonly double step;
        private readonly double precision;
        private readonly uint amount;
        private readonly bool lookingMax;
        private uint counter;

        public lr3(
            Func<double, double> function,
            double start = 0,
            double step = 1E-02,
            double precision = 1E-04,
            uint operationsAmount = 150,
            bool lookingForMax = false
        )
        {
            func = function;
            this.start = start;
            this.step = function(start + step) > function(start - step)
                ? lookingForMax ? step : -step
                : lookingForMax ? -step : step;

            this.precision = precision;
            amount = operationsAmount;
            lookingMax = lookingForMax;
            ResetCounter();
        }

        public double GetOptimal()
        {
            var triplet = GetArea();
            while (!LookingSmall(triplet))
            {
                if (IsCountDown())
                    throw new ApplicationException("Unable to find optimal solution");

                triplet = FindNeedNum(triplet);
            }

            return triplet.A + (triplet.B - triplet.A) / 2;
        }

        public GoldenTriplet GetArea()
        {
            var step = this.step;
            var triplet = new GoldenTriplet { A = start, RightCenter = start + step };

            while (!LookingLucky(triplet))
            {
                if (IsCountDown())
                    throw new ApplicationException("Unable to find search area");

                triplet.LeftCenter = triplet.B;
            }

            ResetCounter();
            return triplet;
        }

        private void ResetCounter() =>
            counter = 0;

        private bool LookingLucky(GoldenTriplet triplet) =>
            lookingMax
                ? func(triplet.RightCenter) >= func(triplet.A) &&
                  func(triplet.RightCenter) >= func(triplet.B)
                : func(triplet.RightCenter) <= func(triplet.A) &&
                  func(triplet.RightCenter) <= func(triplet.B);

        private bool LookingSmall(GoldenTriplet triplet) =>
            Abs(triplet.B - triplet.A) <= precision;

        public GoldenTriplet FindNeedNum(GoldenTriplet old)
        {
            if (lookingMax)
                return func(old.LeftCenter) > func(old.RightCenter)
                    ? new GoldenTriplet { A = old.LeftCenter, B = old.B }
                    : new GoldenTriplet { A = old.A, B = old.RightCenter };
            else
                return func(old.LeftCenter) < func(old.RightCenter)
                    ? new GoldenTriplet { A = old.A, B = old.RightCenter }
                    : new GoldenTriplet { A = old.LeftCenter, B = old.B };
        }

        private bool IsCountDown() =>
            counter++ > amount;
    }

    public class GoldenTriplet
    {
        public const double SmallGolden = 0.3819660113;
        public const double Golden = 0.6180339887;
        public const double BigGolden = 1.6180339887;

        public double A { get; set; }
        public double B { get; set; }

        public double RightCenter
        {
            get => A + (B - A) * Golden;
            set => B = value * BigGolden - A * Golden;
        }

        public double LeftCenter
        {
            get => B - (B - A) * Golden;
            set => B = value + BigGolden * (value - A);
        }
    }

    class Program
    {
        static void Main()
        {
            var func1 = new lr3(x => Pow(x, 6) - 22 * Pow(x, 5) + 22 * Pow(x, 3) - 10 * Pow(x, 2) + x, start: 15);

            var triplet1 = func1.GetArea();

            Console.WriteLine("Задание 1");

            for (int i = 1; i <= 35; i++)
            {
                Console.WriteLine($"{i}\t" +
                    $"{triplet1.A.ToString("F5", CultureInfo.InvariantCulture)}\t" +
                    $"{triplet1.LeftCenter.ToString("F5", CultureInfo.InvariantCulture)}\t" +
                    $"{triplet1.RightCenter.ToString("F5", CultureInfo.InvariantCulture)}\t" +
                    $"{triplet1.B.ToString("F5", CultureInfo.InvariantCulture)}");

                triplet1 = func1.FindNeedNum(triplet1);
            }

            Console.WriteLine("\nЗадание 2");

            var func2 = new lr3(t =>
                Pow(2 - t, 2) + 22 * Pow(1 + 2 * t, 2) + Pow(22 + t, 2) +
                3 * (2 - t) * (1 + 2 * t) - 22 * (2 - t) * (22 + t) - (1 + 2 * t) * (22 + t) + (2 - t) - 22 * (1 + 2 * t) + (22 + t),
                start: -3.0);

            var triplet2 = func2.GetArea();

            for (int i = 1; i <= 30; i++)
            {
                Console.WriteLine($"{i}\t" +
                    $"{triplet2.A.ToString("F5", CultureInfo.InvariantCulture)}\t" +
                    $"{triplet2.LeftCenter.ToString("F5", CultureInfo.InvariantCulture)}\t" +
                    $"{triplet2.RightCenter.ToString("F5", CultureInfo.InvariantCulture)}\t" +
                    $"{triplet2.B.ToString("F5", CultureInfo.InvariantCulture)}");

                triplet2 = func2.FindNeedNum(triplet2);
            }
        }
    }
}