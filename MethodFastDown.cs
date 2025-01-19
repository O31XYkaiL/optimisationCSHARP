namespace MethodFastDown
{
    public class Vector
    {
        public double[] Values { get; }

        public int Length => Values.Length;

        public Vector(int size)
        {
            Values = new double[size];
        }

        public Vector(double[] values)
        {
            Values = values;
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            if (v1.Length != v2.Length)
                throw new InvalidOperationException("Vectors must have the same length.");

            return new Vector(v1.Values.Zip(v2.Values, (x, y) => x + y).ToArray());
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            if (v1.Length != v2.Length)
                throw new InvalidOperationException("Vectors must have the same length.");

            return new Vector(v1.Values.Zip(v2.Values, (x, y) => x - y).ToArray());
        }

        public static Vector operator *(Vector v, double scalar)
        {
            return new Vector(v.Values.Select(x => x * scalar).ToArray());
        }

        public static double Dot(Vector v1, Vector v2)
        {
            if (v1.Length != v2.Length)
                throw new InvalidOperationException("Vectors must have the same length.");

            return v1.Values.Zip(v2.Values, (x, y) => x * y).Sum();
        }

        public double Norm() => Math.Sqrt(Values.Select(x => x * x).Sum());
    }

    public class Matrix
    {
        public double[,] Values { get; }

        public int Rows => Values.GetLength(0);
        public int Columns => Values.GetLength(1);

        public Matrix(int rows, int columns)
        {
            Values = new double[rows, columns];
        }

        public Vector Multiply(Vector vector)
        {
            if (Columns != vector.Length)
                throw new InvalidOperationException("Matrix columns must equal vector length.");

            var result = new double[Rows];
            for (int i = 0; i < Rows; i++)
            {
                result[i] = 0;
                for (int j = 0; j < Columns; j++)
                {
                    result[i] += Values[i, j] * vector.Values[j];
                }
            }
            return new Vector(result);
        }

        public Matrix Transpose()
        {
            var transposed = new Matrix(Columns, Rows);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    transposed.Values[j, i] = Values[i, j];
                }
            }
            return transposed;
        }
    }

    public class MethodFastDown
    {
        public static double Function(Vector v)
        {
            double x = v.Values[0];
            double y = v.Values[1];
            return 100 * Math.Pow(y - x * x, 2) + Math.Pow(1 - x, 2);
        }

        public static Vector Gradient(Vector v)
        {
            double x = v.Values[0];
            double y = v.Values[1];

            double dfdx = -400 * x * (y - x * x) - 2 * (1 - x);
            double dfdy = 200 * (y - x * x);

            return new Vector(new double[] { dfdx, dfdy });
        }

        public static Vector Minimize(Func<Vector, double> function, Func<Vector, Vector> gradient, Vector startPoint, double learningRate, int maxIterations, double tolerance = 1e-6)
        {
            Vector currentPoint = startPoint;

            for (int i = 0; i < maxIterations; i++)
            {
                Vector grad = gradient(currentPoint);

                if (grad.Norm() < tolerance)
                {
                    Console.WriteLine($"Converged at iteration {i}, gradient norm is small.");
                    break;
                }

                currentPoint = currentPoint - grad * learningRate;

                if (i % 100 == 0)
                {
                    Console.WriteLine($"Iteration {i}: f({currentPoint.Values[0]}, {currentPoint.Values[1]}) = {function(currentPoint)}");
                }
            }

            return currentPoint;
        }
    }

    public class Example
    {
        public static void Main(string[] args)
        {
            Vector startPoint = new Vector(new double[] { -1.2, 1 });

            Vector result = MethodFastDown.Minimize(MethodFastDown.Function, MethodFastDown.Gradient, startPoint, 0.001, 10000);
        }
    }

}
