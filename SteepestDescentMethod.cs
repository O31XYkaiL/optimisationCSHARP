namespace SteepestDescentMethod
{
    public class Vector
    {
        private readonly double[] _elements;

        public Vector(IEnumerable<double> elements)
        {
            _elements = elements.ToArray();
        }

        public int Size => _elements.Length;

        public double this[int index] => _elements[index];

        public Vector Add(Vector other) =>
            new Vector(_elements.Zip(other._elements, (a, b) => a + b));

        public Vector Subtract(Vector other) =>
            new Vector(_elements.Zip(other._elements, (a, b) => a - b));

        public Vector Scale(double factor) =>
            new Vector(_elements.Select(e => e * factor));

        public double Length() => Math.Sqrt(_elements.Sum(e => e * e));

        public override string ToString() =>
            $"[{string.Join(", ", _elements.Select(e => e.ToString("F4")))}]";
    }

    public class GradientDescentSolver
    {
        private readonly Func<Vector, double> _objectiveFunction;
        private readonly Func<Vector, Vector> _gradientFunction;

        public GradientDescentSolver(Func<Vector, double> function, Func<Vector, Vector> gradient)
        {
            _objectiveFunction = function;
            _gradientFunction = gradient;
        }

        public (Vector optimalPoint, double optimalValue) Minimize(Vector initialGuess, double tolerance = 1e-6, int maxSteps = 1000)
        {
            Vector currentPoint = initialGuess;

            for (int step = 0; step < maxSteps; step++)
            {
                Vector gradient = _gradientFunction(currentPoint);

                if (gradient.Length() < tolerance)
                    return (currentPoint, _objectiveFunction(currentPoint));

                double stepSize = FindStepSize(currentPoint, gradient);
                currentPoint = currentPoint.Subtract(gradient.Scale(stepSize));
            }

            throw new InvalidOperationException("Convergence not achieved within the maximum allowed steps.");
        }

        private double FindStepSize(Vector point, Vector gradient)
        {
            double start = 0, end = 1, precision = 1e-6;

            while (end - start > precision)
            {
                double mid1 = start + (end - start) / 3;
                double mid2 = end - (end - start) / 3;

                double value1 = _objectiveFunction(point.Subtract(gradient.Scale(mid1)));
                double value2 = _objectiveFunction(point.Subtract(gradient.Scale(mid2)));

                if (value1 < value2)
                    end = mid2;
                else
                    start = mid1;
            }

            return (start + end) / 2;
        }
    }
}
