using static System.Math;

namespace NewtonMethod
{
    public class NewtonMethod
    {
        private readonly Func<double, double> _objectiveFunction;
        private readonly DerivativeCalculator _derivativeCalculator;
        private readonly double _initialGuess;
        private readonly double _tolerance;
        private readonly int _maxIterations;
        private int _iterationCount;

        public NewtonMethod(
            Func<double, double> objectiveFunction,
            double initialGuess = 0,
            double tolerance = 1E-08,
            int maxIterations = 50
        )
        {
            _objectiveFunction = objectiveFunction;
            _derivativeCalculator = new DerivativeCalculator(objectiveFunction);
            _initialGuess = initialGuess;
            _tolerance = tolerance;
            _maxIterations = maxIterations;
            ResetIterationCount();
        }

        public double Optimize()
        {
            var firstDerivative = _derivativeCalculator.CalculateFirstDerivative(_initialGuess);
            var secondDerivative = _derivativeCalculator.CalculateSecondDerivative(_initialGuess);
            var currentPoint = _initialGuess - firstDerivative / secondDerivative;
            var previousPoint = _initialGuess;

            Console.WriteLine($"{_iterationCount} {_initialGuess} {firstDerivative}, {secondDerivative} {currentPoint}");

            while (!MathTools.AreApproximatelyEqual(firstDerivative, 0) && !HasConverged(previousPoint, currentPoint))
            {
                if (HasExceededMaxIterations())
                    throw new ApplicationException("Solution not found within the maximum number of iterations.");

                previousPoint = currentPoint;
                firstDerivative = _derivativeCalculator.CalculateFirstDerivative(currentPoint);
                secondDerivative = _derivativeCalculator.CalculateSecondDerivative(currentPoint);
                currentPoint = currentPoint - firstDerivative / secondDerivative;

                Console.WriteLine($"{_iterationCount} {previousPoint} {firstDerivative} {secondDerivative} {currentPoint}");
            }

            return currentPoint;
        }

        private bool HasExceededMaxIterations() => _iterationCount++ > _maxIterations;

        private void ResetIterationCount() => _iterationCount = 0;

        private bool HasConverged(double previousPoint, double currentPoint) =>
            Abs(_objectiveFunction(currentPoint) - _objectiveFunction(previousPoint)) < _tolerance;
    }

    public class DerivativeCalculator
    {
        private readonly Func<double, double> _function;

        public DerivativeCalculator(Func<double, double> function) =>
            _function = function;

        public double CalculateFirstDerivative(double point)
        {
            var epsilon = MathTools.GetMinimalEpsilonForPoint(point);
            var leftValue = _function(point + epsilon);
            var rightValue = _function(point - epsilon);
            return (leftValue - rightValue) / (2 * epsilon);
        }

        public double CalculateSecondDerivative(double point)
        {
            var epsilon = MathTools.GetMinimalEpsilonForPoint(point);
            var leftValue = _function(point + 2 * epsilon);
            var rightValue = _function(point - 2 * epsilon);
            return (leftValue + rightValue - 2 * _function(point)) / (4 * epsilon * epsilon);
        }
    }

    public static class MathTools
    {
        public const int DefaultPower = 5;

        public static bool AreApproximatelyEqual(double a, double b)
        {
            var minimalEpsilon = Max(GetMinimalEpsilonForPoint(a), GetMinimalEpsilonForPoint(b));
            return Abs(a - b) < minimalEpsilon;
        }

        public static int GetMinimalPower(double n, bool restrictMaxPower = true)
        {
            if (n == 0) return DefaultPower;

            var absoluteValue = Abs(n);
            var logarithm = Log10(absoluteValue);
            var rounded = Round(logarithm);
            var result = (int)Abs(rounded - DefaultPower);
            result = restrictMaxPower && result > 15 ? 15 : result;

            return result;
        }

        public static double GetMinimalEpsilonForPoint(double n) =>
            Pow(10, -1 * GetMinimalPower(n));
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Оптимизация функции F(x) = x^6 – 8x^5 + 8x^3 – 10x^2 + x:");
            var optimizer1 = new NewtonMethod(
                x => Math.Pow(x, 6) - 8 * Math.Pow(x, 5) + 8 * Math.Pow(x, 3) - 10 * Math.Pow(x, 2) + x,
                initialGuess: 6
            );

            try
            {
                var optimal1 = optimizer1.Optimize();
                Console.WriteLine($"Найденная точка: {optimal1}");
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine($"Error for the first function: {ex.Message}");
            }

            Console.WriteLine();

            Console.WriteLine("Оптимизация функции перехода g(t)=(2-t)^2+8(1+2t)^2+(8+t)^2+3(2-t)(1+2t)-8(2-t)(8+t)-(1+2t)(8+t)+(8-t)-8(1+2t)+(8+t):");
            var optimizer2 = new NewtonMethod(
                t => Math.Pow(2 - t, 2) + 8 * Math.Pow(1 + 2 * t, 2) + Math.Pow(8 + t, 2) +
                     3 * (2 - t) * (1 + 2 * t) - 8 * (2 - t) * (8 + t) -
                     (1 + 2 * t) * (8 + t) + (2 - t) - 8 * (1 + 2 * t) + (8 + t),
                initialGuess: -2
            );

            try
            {
                var optimal2 = optimizer2.Optimize();
                Console.WriteLine($"Найденная тчока: {optimal2}");
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine($"Error for the second function: {ex.Message}");
            }
        }
    }
}