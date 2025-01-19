using static System.Math;

namespace NumericalAnalysisTask
{
    public class NumericalAnalysisTask
    {
        private static readonly double[] ExponentialValues = new double[]
        {
        1, 1E-01, 1E-02, 1E-03, 1E-04, 1E-05, 1E-06, 1E-07,
        1E-08, 1E-09, 1E-10, 1E-11, 1E-12, 1E-13, 1E-14, 1E-15
        };

        public Func<double, double> TargetFunction { get; set; }

        public NumericalAnalysisTask(Func<double, double> function)
        {
            TargetFunction = function;
        }

        public bool IsBaseCaseValid() => TargetFunction(0) == 0;

        public DataPoint[] GenerateDataPoints(uint startIndex = 0, uint endIndex = 16)
        {
            if (endIndex > 16 || startIndex + endIndex > 16)
                throw new ArgumentException("Invalid range");

            var results = new DataPoint[endIndex - startIndex];

            for (int i = (int)startIndex, j = 0; i < 16 && j < endIndex - startIndex; i++, j++)
            {
                results[j] = new DataPoint
                {
                    Value = ExponentialValues[i],
                    FunctionValue = TargetFunction(ExponentialValues[i])
                };
            }

            return results;
        }

        public double ComputeConstant() => Pow(10, ComputeSlope().Exponent);

        public SlopeParameters ComputeSlope() => new SlopeParameters(TargetFunction);
    }

    public class SlopeParameters
    {
        public double Coefficient { get; private set; }
        public double Exponent { get; private set; }

        public SlopeParameters(Func<double, double> function)
        {
            double r = Pow(10, -4);
            double l = Pow(10, -4.001);

            Coefficient = ComputeLogRatio(function(r), function(l)) / Log10(r / l);
            Exponent = -Log10(r) * ComputeLogRatio(function(l), function(r)) / Log10(l / r) + ComputeLogRatio(function(r), 1);
        }

        private double ComputeLogRatio(double numerator, double denominator) =>
            numerator == 0 ? double.NegativeInfinity : Log10(Abs(numerator / denominator));
    }

    public struct DataPoint
    {
        public double Value { get; set; }
        public double FunctionValue { get; set; }

        public double LogValue => Log10(Abs(Value));
        public double LogFunctionValue => Log10(Abs(FunctionValue));
    }

    public class SortingAnalysis
    {
        private readonly NumericalAnalysisTask analysisTask;
        private readonly ISortingAlgorithm sorter;

        public SortingAnalysis()
        {
            analysisTask = new NumericalAnalysisTask(x => 1.0 / CountSortOperations((int)Round(1 / x)));
            sorter = new BubbleSortAlgorithm();
        }

        public DataPoint[] GenerateData() => analysisTask.GenerateDataPoints(1, 6);

        public double ComputeConstant() => analysisTask.ComputeConstant();

        public SlopeParameters ComputeSlope() => analysisTask.ComputeSlope();

        private int CountSortOperations(int items)
        {
            var array = new int[items];
            for (int i = 0; i < items; i++)
            {
                array[i] = i;
            }
            return sorter.Sort(array);
        }
    }

    public interface ISortingAlgorithm
    {
        int Sort(int[] array);
    }

    public class BubbleSortAlgorithm : ISortingAlgorithm
    {
        public int Sort(int[] array)
        {
            int operations = 0;
            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - i - 1; j++)
                {
                    if (array[j + 1] > array[j])
                    {
                        Swap(ref array[j], ref array[j + 1]);
                        operations++;
                    }
                }
            }
            return operations;
        }

        private static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
    }

    public class MultivariateAnalysis
    {
        private readonly Func<Vector, double> multivariateFunction;
        private readonly Vector referencePoint;
        private readonly Vector directionVector;
        private readonly NumericalAnalysisTask functionAnalyzer;

        public MultivariateAnalysis(Func<Vector, double> function, Vector point, Vector vector)
        {
            multivariateFunction = function;
            referencePoint = point;
            directionVector = vector;
            functionAnalyzer = new NumericalAnalysisTask(FunctionWrapper);
        }

        public DataPoint[] GenerateData() => functionAnalyzer.GenerateDataPoints();
        public double ComputeConstant() => functionAnalyzer.ComputeConstant();
        public SlopeParameters ComputeSlope() => functionAnalyzer.ComputeSlope();

        public Vector[] GenerateCirclePoints(int iterations = 180)
        {
            var rotationMatrix = MathUtils.CreateRotationMatrix(PI / 90);
            var points = new Vector[iterations];
            var currentVector = directionVector;

            for (int i = 0; i < iterations; i++)
            {
                functionAnalyzer.TargetFunction = t => multivariateFunction(referencePoint + t * currentVector) - multivariateFunction(referencePoint);
                points[i] = referencePoint + functionAnalyzer.ComputeConstant() * currentVector;
                currentVector = currentVector * rotationMatrix;
            }

            return points;
        }

        private double FunctionWrapper(double t) => multivariateFunction(referencePoint + t * directionVector) - multivariateFunction(referencePoint);
    }

    public static class MathUtils
    {
        public static Matrix CreateRotationMatrix(double angle)
        {
            return new Matrix(new double[][]
            {
            new double[] { Cos(angle), -Sin(angle) },
            new double[] { Sin(angle), Cos(angle) }
            });
        }
    }

    public class Vector
    {
        private double[] Components { get; set; }

        public Vector(params double[] components) => Components = components;

        public override string ToString() => string.Join("\t", Components);

        public double GetComponent(int index) => Components[index];

        public Vector Orthogonal() => new Vector(GetComponent(1), -GetComponent(0));

        public int Dimension => Components.Length;

        public static Vector operator +(Vector v1, Vector v2)
        {
            if (v1.Dimension != v2.Dimension)
                throw new InvalidOperationException("Dimension mismatch");

            var result = new double[v1.Dimension];
            for (int i = 0; i < v1.Dimension; i++)
                result[i] = v1.GetComponent(i) + v2.GetComponent(i);

            return new Vector(result);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            if (v1.Dimension != v2.Dimension)
                throw new InvalidOperationException("Dimension mismatch");

            var result = new double[v1.Dimension];
            for (int i = 0; i < v1.Dimension; i++)
                result[i] = v1.GetComponent(i) - v2.GetComponent(i);

            return new Vector(result);
        }

        public static double operator *(Vector v1, Vector v2)
        {
            if (v1.Dimension != v2.Dimension)
                throw new InvalidOperationException("Dimension mismatch");

            double result = 0;
            for (int i = 0; i < v1.Dimension; i++)
                result += v1.GetComponent(i) * v2.GetComponent(i);

            return result;
        }

        public static Vector operator *(double scalar, Vector vector)
        {
            var result = new double[vector.Dimension];
            for (int i = 0; i < vector.Dimension; i++)
                result[i] = scalar * vector.GetComponent(i);

            return new Vector(result);
        }

        public static Vector operator *(Vector vector, Matrix matrix)
        {
            if (vector.Dimension != matrix.Dimension)
                throw new InvalidOperationException("Dimension mismatch");

            var result = new double[vector.Dimension];
            for (int i = 0; i < vector.Dimension; i++)
            {
                result[i] = 0;
                for (int j = 0; j < vector.Dimension; j++)
                    result[i] += vector.GetComponent(j) * matrix.GetElement(j, i);
            }

            return new Vector(result);
        }
    }

    public class Matrix
    {
        private double[][] Elements { get; set; }

        public Matrix(double[][] elements)
        {
            if (elements.Length != elements[0].Length)
                throw new ArgumentException("Invalid matrix dimensions");

            Elements = elements;
        }

        public double GetElement(int row, int col) => Elements[row][col];

        public int Dimension => Elements.Length;

        public override string ToString()
        {
            return string.Join("\n", Elements.Select(row => string.Join(", ", row)));
        }

        public static Vector operator *(Matrix matrix, Vector vector)
        {
            var result = new double[vector.Dimension];
            for (int i = 0; i < vector.Dimension; i++)
            {
                result[i] = new Vector(matrix.Elements[i]) * vector;
            }
            return new Vector(result);
        }

        public Matrix Inverse()
        {
            double determinant = GetDeterminant();
            var inverseElements = new double[][]
            {
            new double[] { Elements[1][1] / determinant, -Elements[0][1] / determinant },
            new double[] { -Elements[1][0] / determinant, Elements[0][0] / determinant }
            };
            return new Matrix(inverseElements);
        }

        private double GetDeterminant() => Elements[0][0] * Elements[1][1] - Elements[0][1] * Elements[1][0];
    }

    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Task 2.1:");
            ExecuteTask1();

            Console.WriteLine("\nTask 2.2:");
            ExecuteTask2();

            Console.WriteLine("\nTask 4.1:");
            ExecuteTask3();

            Console.WriteLine("\nTask 4.2:");
            ExecuteTask4();

            Console.WriteLine("\nTask 4.3:");
            ExecuteTask5();
        }

        private static void ExecuteTask1()
        {
            var task = new NumericalAnalysisTask(x => 3 * Pow(x, 1.5));

            var data = task.GenerateDataPoints(endIndex: 16);

            Console.WriteLine("| Value (N)     | Function (F)  | Log10(N)      | Log10(F)      |");
            Console.WriteLine("|---------------|---------------|---------------|---------------|");

            foreach (var point in data)
            {
                Console.WriteLine($"| {point.Value,13:E2} | {point.FunctionValue,13:E2} | {point.LogValue,13:F4} | {point.LogFunctionValue,13:F4} |");
            }

            Console.WriteLine($"a: {task.ComputeSlope().Coefficient:F4}");
            Console.WriteLine($"Ct: {task.ComputeConstant():F4}");
        }

        private static void ExecuteTask2()
        {
            var analyzer = new SortingAnalysis();

            Console.WriteLine("| N             | Function (F)  | Log10(N)      | Log10(F)      |");
            Console.WriteLine("|---------------|---------------|---------------|---------------|");

            for (int N = 100; N <= 1000; N += 100)
            {
                double F = analyzer.ComputeConstant() * Pow(N, analyzer.ComputeSlope().Exponent);

                Console.WriteLine($"| {N,13} | {F,13:E2} | {Log10(N),13:F4} | {Log10(Abs(F)),13:F4} |");
            }

            Console.WriteLine($"a: {analyzer.ComputeSlope().Exponent:F4}");
            Console.WriteLine($"C: {analyzer.ComputeConstant():F4}");
        }

        private static void ExecuteTask3()
        {
            Func<Vector, double> function = v => v.GetComponent(0) * v.GetComponent(0) - 22 * Sqrt(v.GetComponent(0) * v.GetComponent(1));

            Vector point = new Vector(1, 1);
            Vector direction = new Vector(1.0 / Sqrt(1 + 22 * 22 / 4), 11 / Sqrt(1 + 22 * 22 / 4));

            var task = new MultivariateAnalysis(function, point, direction);

            Console.WriteLine("| Value (N)     | Function (F)  | Log10(N)      | Log10(F)      |");
            Console.WriteLine("|---------------|---------------|---------------|---------------|");

            var data = task.GenerateData();
            foreach (var pointData in data)
            {
                Console.WriteLine($"| {pointData.Value,13} | {pointData.FunctionValue,13:E2} | {pointData.LogValue,13:F4} | {pointData.LogFunctionValue,13:F4} |");
            }

            Console.WriteLine($"a: {task.ComputeSlope().Coefficient:F4}");
            Console.WriteLine($"C: {task.ComputeConstant():F4}");
        }

        private static void ExecuteTask4()
        {
            Func<Vector, double> function = v => v.GetComponent(0) * v.GetComponent(0) - 22 * Sqrt(v.GetComponent(0) * v.GetComponent(1));

            Vector point = new Vector(1, 1);
            Vector gradient = new Vector(2 * point.GetComponent(0) - 22 * Sqrt(point.GetComponent(0) * point.GetComponent(1)),
                                         11 / Sqrt(point.GetComponent(0) * point.GetComponent(1)));
            Vector orthogonal = gradient.Orthogonal();

            var task = new MultivariateAnalysis(function, point, orthogonal);

            Console.WriteLine("| T             | G             | Log10(T)      | Log10(G)      |");
            Console.WriteLine("|---------------|---------------|---------------|---------------|");

            var data = task.GenerateData();
            foreach (var pointData in data)
            {
                Console.WriteLine($"| {pointData.Value,13} | {pointData.FunctionValue,13:E2} | {pointData.LogValue,13:F4} | {pointData.LogFunctionValue,13:F4} |");
            }

            Console.WriteLine($"a: {task.ComputeSlope().Coefficient:F4}");
            Console.WriteLine($"C: {task.ComputeConstant():F4}");
        }

        private static void ExecuteTask5()
        {
            Func<Vector, double> function = v => v.GetComponent(0) * v.GetComponent(0) - 22 * Sqrt(v.GetComponent(0) * v.GetComponent(1));

            Vector point = new Vector(1, 1);
            Vector direction = new Vector(1.0 / Sqrt(1 + 22 * 22 / 4), 11 / Sqrt(1 + 22 * 22 / 4));

            var task = new MultivariateAnalysis(function, point, direction);

            Vector[] circlePoints = task.GenerateCirclePoints();

            Console.WriteLine("| X | Y |");
            Console.WriteLine("|---|---|");

            foreach (var circlePoint in circlePoints)
            {
                Console.WriteLine(
                    $"{circlePoint.GetComponent(0).ToString("F4", System.Globalization.CultureInfo.InvariantCulture)} " +
                    $"{circlePoint.GetComponent(1).ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}"
                );
            }
        }
    }
}