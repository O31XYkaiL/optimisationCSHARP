namespace GradientProjection
{
    class GradientProjection
    {
        public static double CalculateObjective(double x, double y)
        {
            return Math.Pow(x - 100, 2) + Math.Pow(y - 10, 2);
        }

        public static (double dx, double dy) CalculateGradient(double x, double y)
        {
            return (2 * (x - 100), 2 * (y - 10));
        }

        public static (double x, double y) EnforceBounds(double x, double y, double minBound, double maxBound)
        {
            x = Math.Max(minBound, Math.Min(x, maxBound));
            y = Math.Max(minBound, Math.Min(y, maxBound));
            return (x, y);
        }

        public static void ExecuteOptimization(double startX, double startY, double stepSize, int maxSteps)
        {
            double currentX = startX;
            double currentY = startY;

            for (int step = 1; step <= maxSteps; step++)
            {
                var (gradX, gradY) = CalculateGradient(currentX, currentY);
                currentX -= stepSize * gradX;
                currentY -= stepSize * gradY;

                var (boundedX, boundedY) = EnforceBounds(currentX, currentY, 0, 200);
                currentX = boundedX;
                currentY = boundedY;

                double objectiveValue = CalculateObjective(currentX, currentY);
                Console.WriteLine($"Step {step}: x = {currentX:F4}, y = {currentY:F4}, Objective = {objectiveValue:F6}");

                if (Math.Sqrt(gradX * gradX + gradY * gradY) < 1e-6)
                {
                    Console.WriteLine("Convergence achieved!");
                    break;
                }
            }
        }

        static void Main()
        {
            double initialX = 50.0;
            double initialY = 50.0;
            double learningRate = 0.1;
            int maximumIterations = 50;

            Console.WriteLine("Launching optimization process...");
            ExecuteOptimization(initialX, initialY, learningRate, maximumIterations);
        }
    }

}
