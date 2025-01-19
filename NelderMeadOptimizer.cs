namespace NelderMeadOptimizer
{
    public static class NelderMeadOptimizer
    {
        public static Vector Optimize(Func<Vector, double> objective, Vector[] startingSimplex, double precision = 1e-6, int maxSteps = 1000)
        {
            if (startingSimplex.Length < 3)
                throw new ArgumentException("Начальная фигура должна содержать минимум три точки.");

            int dimension = startingSimplex[0].Dimension;
            if (startingSimplex.Any(point => point.Dimension != dimension))
                throw new ArgumentException("Все точки начальной фигуры должны быть одинаковой размерности.");

            Vector[] simplex = startingSimplex;
            int steps = 0;

            while (steps < maxSteps)
            {
                Array.Sort(simplex, (a, b) => objective(a).CompareTo(objective(b)));

                double maxDistance = simplex.Max(point => point.CalculateDistance(simplex[0]));
                if (maxDistance < precision)
                    break;

                Vector barycenter = Vector.CalculateCentroid(simplex.Take(simplex.Length - 1).ToArray());

                Vector reflected = barycenter + (barycenter - simplex[^1]) * 1.0;
                if (objective(reflected) < objective(simplex[0]))
                {
                    Vector expanded = barycenter + (reflected - barycenter) * 2.0;
                    simplex[^1] = objective(expanded) < objective(reflected) ? expanded : reflected;
                }
                else if (objective(reflected) < objective(simplex[^2]))
                {
                    simplex[^1] = reflected;
                }
                else
                {
                    Vector contracted = barycenter + (simplex[^1] - barycenter) * 0.5;
                    if (objective(contracted) < objective(simplex[^1]))
                    {
                        simplex[^1] = contracted;
                    }
                    else
                    {
                        for (int i = 1; i < simplex.Length; i++)
                        {
                            simplex[i] = simplex[0] + (simplex[i] - simplex[0]) * 0.5;
                        }
                    }
                }

                steps++;
            }

            return simplex[0];
        }
    }

    public class Vector
    {
        public double[] Elements { get; }

        public int Dimension => Elements.Length;

        public Vector(params double[] elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public static Vector operator +(Vector a, Vector b)
        {
            if (a.Dimension != b.Dimension)
                throw new ArgumentException("Размерности векторов должны совпадать.");

            return new Vector(a.Elements.Zip(b.Elements, (x, y) => x + y).ToArray());
        }

        public static Vector operator -(Vector a, Vector b)
        {
            if (a.Dimension != b.Dimension)
                throw new ArgumentException("Размерности векторов должны совпадать.");

            return new Vector(a.Elements.Zip(b.Elements, (x, y) => x - y).ToArray());
        }

        public static Vector operator *(Vector a, double multiplier)
        {
            return new Vector(a.Elements.Select(x => x * multiplier).ToArray());
        }

        public double CalculateDistance(Vector other)
        {
            if (Dimension != other.Dimension)
                throw new ArgumentException("Размерности векторов должны совпадать.");
            return Math.Sqrt(Elements.Zip(other.Elements, (x, y) => Math.Pow(x - y, 2)).Sum());
        }

        public static Vector CalculateCentroid(params Vector[] points)
        {
            if (points.Length == 0)
                throw new ArgumentException("Должен быть передан хотя бы один вектор.");

            int dimension = points[0].Dimension;
            if (points.Any(v => v.Dimension != dimension))
                throw new ArgumentException("Все векторы должны быть одинаковой размерности.");

            double[] sum = new double[dimension];
            foreach (var point in points)
            {
                for (int i = 0; i < dimension; i++)
                    sum[i] += point.Elements[i];
            }

            return new Vector(sum.Select(x => x / points.Length).ToArray());
        }

        public override string ToString() => $"[{string.Join(", ", Elements.Select(e => e.ToString("F4")))}]";
    }

    class Program
    {
        static void Main()
        {
            Func<Vector, double> objectiveFunction = coordinates =>
            {
                double x = coordinates.Elements[0];
                double y = coordinates.Elements[1];
                return -x * x * Math.Exp(1 - x * x - Math.Pow(x - y, 2));
            };

            Vector[][] initialConfigurations = {
            new[] { new Vector(0.5, 0.5), new Vector(1.0, 0.5), new Vector(0.5, 1.0) },
            new[] { new Vector(-0.5, -0.5), new Vector(-1.0, -0.5), new Vector(-0.5, -1.0) }
        };

            double accuracy = 1e-9;
            int maxIterations = 10000;

            for (int region = 0; region < initialConfigurations.Length; region++)
            {
                Vector optimum = NelderMeadOptimizer.Optimize(objectiveFunction, initialConfigurations[region], accuracy, maxIterations);
                Console.WriteLine($"Локальный минимум в области {region + 1} найден в точке: {optimum}");
                Console.WriteLine($"Значение функции в этой точке: {objectiveFunction(optimum)}");
            }
        }
    }

}