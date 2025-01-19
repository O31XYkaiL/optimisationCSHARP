namespace StepDivisionOptimization
{
    public static class StepDivisionOptimization
    {
        private static Vector ComputeGradient(Func<Vector, double> objective, Vector point, double delta = 1e-6)
        {
            double[] gradient = new double[point.Dimension];
            for (int i = 0; i < point.Dimension; i++)
            {
                double[] shift = new double[point.Dimension];
                shift[i] = delta;

                double forwardValue = objective(point.Add(new Vector(shift)));
                double backwardValue = objective(point.Subtract(new Vector(shift)));

                gradient[i] = (forwardValue - backwardValue) / (2 * delta);
            }
            return new Vector(gradient);
        }

        public static Vector FindMinimum(Func<Vector, double> objective, Vector startPoint, double stepSize, double reductionFactor, double tolerance)
        {
            Vector currentPoint = startPoint;
            double currentStep = stepSize;

            while (currentStep > tolerance)
            {
                Vector grad = ComputeGradient(objective, currentPoint);
                Vector potentialPoint = currentPoint.Subtract(grad.Scale(currentStep));

                if (objective(potentialPoint) < objective(currentPoint))
                {
                    currentPoint = potentialPoint;
                }
                else
                {
                    currentStep *= reductionFactor;
                }
            }

            return currentPoint;
        }

        public class Vector
        {
            public double[] Coordinates { get; }

            public int Dimension => Coordinates.Length;

            public Vector(params double[] coordinates)
            {
                Coordinates = coordinates;
            }

            public double Magnitude()
            {
                return Math.Sqrt(Coordinates.Sum(c => c * c));
            }

            public Vector Add(Vector other)
            {
                if (Dimension != other.Dimension)
                    throw new ArgumentException("Vector dimensions must match.");
                return new Vector(Coordinates.Zip(other.Coordinates, (a, b) => a + b).ToArray());
            }

            public double DotProduct(Vector other)
            {
                if (Dimension != other.Dimension)
                    throw new ArgumentException("Vector dimensions must match.");
                return Coordinates.Zip(other.Coordinates, (a, b) => a * b).Sum();
            }

            public Vector Subtract(Vector other)
            {
                if (Dimension != other.Dimension)
                    throw new ArgumentException("Vector dimensions must match.");
                return new Vector(Coordinates.Zip(other.Coordinates, (a, b) => a - b).ToArray());
            }

            public Vector Scale(double scalar)
            {
                return new Vector(Coordinates.Select(c => c * scalar).ToArray());
            }

            public override string ToString()
            {
                return $"[{string.Join(", ", Coordinates)}]";
            }
        }

        class Program
        {
            static void Main()
            {
                Func<Vector, double> targetFunction = v =>
                {
                    double x = v.Coordinates[0];
                    double y = v.Coordinates[1];
                    return -Math.Pow(y, 2) * Math.Pow(x, 2) * Math.Exp(1 - Math.Pow(x, 2) - Math.Pow(x - y, 2));
                };

                var startPoint1 = new Vector(1.5, 1.5);
                var startPoint2 = new Vector(-1.5, -1.5);

                double initialStep = 1.0;
                double shrinkFactor = 0.5;
                double precision = 1e-6;

                var minimum1 = StepDivisionOptimization.FindMinimum(targetFunction, startPoint1, initialStep, shrinkFactor, precision);
                var minimum2 = StepDivisionOptimization.FindMinimum(targetFunction, startPoint2, initialStep, shrinkFactor, precision);

                Console.WriteLine($"First minimum point: {minimum1}\n" +
                                  $"Minimum value at first point: {targetFunction(minimum1):F6}");

                Console.WriteLine($"Second minimum point: {minimum2}\n" +
                                  $"Minimum value at second point: {targetFunction(minimum2):F6}");
            }
        }
    }

}
