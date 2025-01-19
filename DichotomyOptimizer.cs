using System.ComponentModel;
using OfficeOpenXml;

namespace DichotomyOptimizer
{
    public class DichotomyOptimizer
    {
        private readonly Func<double, double> targetFunction;
        private readonly double initialPoint;
        private readonly double searchStep;
        private readonly double tolerance;
        private readonly uint maxIterations;
        private readonly bool maximize;
        private uint iterationCounter;

        public DichotomyOptimizer(Func<double, double> function, double start = 0.0, double step = 0.01, double precision = 0.0001, uint maxOps = 150, bool maximize = false)
        {
            targetFunction = function;
            initialPoint = start;
            tolerance = precision;
            maxIterations = maxOps;
            this.maximize = maximize;

            // Determine direction based on function value comparison
            searchStep = function(start + step) > function(start - step)
                ? maximize ? step : -step
                : maximize ? -step : step;

            ResetIterationCount();
        }

        public double Optimize()
        {
            var interval = IdentifyInitialRange();
            while (!IsIntervalSufficient(interval))
            {
                if (HasReachedIterationLimit())
                    throw new ApplicationException("Failed to find optimal value within iteration limit.");

                var leftPart = new Interval { Left = interval.Left, Right = interval.Midpoint };
                var rightPart = new Interval { Left = interval.Midpoint, Right = interval.Right };

                interval = ChooseBetterInterval(leftPart, rightPart);
            }
            return interval.Midpoint;
        }

        private Interval IdentifyInitialRange()
        {
            var step = searchStep;
            var range = new Interval { Left = initialPoint, Right = initialPoint + step * 2 };

            while (!IsPeakOrValley(range) && !HasReachedIterationLimit())
            {
                range.Right += step;
                step *= 2; // Exponential step expansion
            }

            if (HasReachedIterationLimit())
                throw new ApplicationException("Unable to locate a suitable interval for search.");

            ResetIterationCount();
            return range;
        }

        private bool IsPeakOrValley(Interval range) =>
            maximize
                ? targetFunction(range.Midpoint) >= targetFunction(range.Left) && targetFunction(range.Midpoint) >= targetFunction(range.Right)
                : targetFunction(range.Midpoint) <= targetFunction(range.Left) && targetFunction(range.Midpoint) <= targetFunction(range.Right);

        private bool IsIntervalSufficient(Interval range) =>
            Math.Abs(range.Right - range.Left) <= tolerance;

        public Interval ChooseBetterInterval(Interval left, Interval right)
        {
            if (!(IsPeakOrValley(left) ^ IsPeakOrValley(right)))
            {
                return maximize
                    ? targetFunction(left.Midpoint) > targetFunction(right.Midpoint) ? left : right
                    : targetFunction(left.Midpoint) < targetFunction(right.Midpoint) ? left : right;
            }
            else if (IsPeakOrValley(left))
            {
                return left;
            }
            else if (IsPeakOrValley(right))
            {
                return right;
            }
            throw new ApplicationException($"Ambiguity in interval selection: {left} vs {right}");
        }

        private bool HasReachedIterationLimit() =>
            iterationCounter++ > maxIterations;

        private void ResetIterationCount() =>
            iterationCounter = 0;
    }

    public class Interval
    {
        public double Left { get; set; }
        public double Right { get; set; }
        public double Midpoint => Left + (Right - Left) / 2;

        public override string ToString() =>
            $"[{Left}, {Midpoint}, {Right}]";
    }

    class Program
    {
        static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var resultsPath = @"C:\Users \Desktop\методы оптимизации\3 лаба";

            var optimizer1 = new DichotomyOptimizer(x => Math.Pow(x, 6) - 21 * Math.Pow(x, 5) + 21 * Math.Pow(x, 3) - 10 * Math.Pow(x, 2) + x);

            var transitionFunction = new Func<double, double>(t =>
                Math.Pow(2 - t, 2) + 21 * Math.Pow(1 + 2 * t, 2) + Math.Pow(21 + t, 2) +
                3 * (2 - t) * (1 + 2 * t) - 21 * (2 - t) * (21 + t) - (1 + 2 * t) * (21 + t) +
                (2 - t) - 21 * (1 + 2 * t) + (21 + t));

            var optimizer2 = new DichotomyOptimizer(transitionFunction);

            using (var excelPackage = new ExcelPackage())
            {
                var sheet1 = excelPackage.Workbook.Worksheets.Add("Task1Results");
                sheet1.Cells[1, 1].Value = "Iteration";
                sheet1.Cells[1, 2].Value = "Left";
                sheet1.Cells[1, 3].Value = "Midpoint";
                sheet1.Cells[1, 4].Value = "Right";

                var interval1 = new Interval { Left = 16, Right = 20 };
                for (int i = 1; i <= 20; i++)
                {
                    sheet1.Cells[i + 1, 1].Value = i;
                    sheet1.Cells[i + 1, 2].Value = interval1.Left;
                    sheet1.Cells[i + 1, 3].Value = interval1.Midpoint;
                    sheet1.Cells[i + 1, 4].Value = interval1.Right;

                    interval1 = optimizer1.ChooseBetterInterval(
                        new Interval { Left = interval1.Left, Right = interval1.Midpoint },
                        new Interval { Left = interval1.Midpoint, Right = interval1.Right });
                }

                var sheet2 = excelPackage.Workbook.Worksheets.Add("Task2Results");
                sheet2.Cells[1, 1].Value = "Iteration";
                sheet2.Cells[1, 2].Value = "Left";
                sheet2.Cells[1, 3].Value = "Midpoint";
                sheet2.Cells[1, 4].Value = "Right";

                var interval2 = new Interval { Left = optimizer2.Optimize(), Right = optimizer2.Optimize() + 0.01 };
                for (int i = 1; i <= 20; i++)
                {
                    sheet2.Cells[i + 1, 1].Value = i;
                    sheet2.Cells[i + 1, 2].Value = interval2.Left;
                    sheet2.Cells[i + 1, 3].Value = interval2.Midpoint;
                    sheet2.Cells[i + 1, 4].Value = interval2.Right;

                    interval2 = optimizer2.ChooseBetterInterval(
                        new Interval { Left = interval2.Left, Right = interval2.Midpoint },
                        new Interval { Left = interval2.Midpoint, Right = interval2.Right });
                }

                string filePath = Path.Combine(resultsPath, "OptimizationResults.xlsx");
                var resultFile = new FileInfo(filePath);
                excelPackage.SaveAs(resultFile);
            }

            Console.WriteLine($"Results saved at: {resultsPath}OptimizationResults.xlsx");
        }
    }
}
