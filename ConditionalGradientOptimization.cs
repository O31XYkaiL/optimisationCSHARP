namespace ConditionalGradientOptimization
{
    class ConditionalGradientOptimization
    {
        // Целевая функция, которую мы хотим минимизировать: F(x, y) = (x - 10)^2 + 100(y - 10)^2
        public static double EvaluateObjective(double x, double y)
        {
            // Вычисляем значение функции для заданных x и y
            return Math.Pow(x - 10, 2) + 100 * Math.Pow(y - 10, 2);
        }

        // Метод для вычисления градиента целевой функции
        public static (double gradX, double gradY) ComputeGradient(double x, double y)
        {
            // Градиент — это вектор частных производных функции F(x, y) по x и y
            double gradX = 2 * (x - 10); // Производная F(x, y) по x
            double gradY = 200 * (y - 10);    // Производная F(x, y) по y
            return (gradX, gradY);     // Возвращаем градиент как кортеж
        }

        // Метод для поиска оптимальной вершины (угловой точки области ограничений)
        public static (double vertexX, double vertexY) FindOptimalVertex(double gradientX, double gradientY)
        {
            // Указываем вершины области ограничений, заданные из графика
            var corners = new (double, double)[]
            {
            (2.72727, 4.90909),
            (0.83333, 4.27778),
            (0.83333, 1.66667),
            (2.72727, 0.90909),
            (6, 2),
            (4.54545, 4.18182)
            };

            // Инициализируем "лучшую" точку и минимальное скалярное произведение
            double bestDotProduct = double.MaxValue; // "Бесконечно большое" число
            (double x, double y) bestVertex = (0, 0); // Начальная точка (можно любая)

            // Перебираем каждую вершину области
            foreach (var (cornerX, cornerY) in corners)
            {
                // Вычисляем скалярное произведение градиента и текущей вершины
                double dotProduct = gradientX * cornerX + gradientY * cornerY;

                // Если скалярное произведение меньше текущего "лучшего", обновляем
                if (dotProduct < bestDotProduct)
                {
                    bestDotProduct = dotProduct;
                    bestVertex = (cornerX, cornerY);
                }
            }

            // Возвращаем оптимальную вершину
            return bestVertex;
        }

        // Основной метод реализации условного градиента
        public static void ExecuteConditionalGradientDescent(
            double initialX,          // Начальное значение x
            double initialY,          // Начальное значение y
            int iterationsLimit,      // Максимальное число итераций
            double convergenceThreshold // Порог сходимости
        )
        {
            // Текущие значения переменных x и y
            double currentX = initialX;
            double currentY = initialY;

            // Основной цикл оптимизации
            for (int step = 1; step <= iterationsLimit; step++)
            {
                // 1. Вычисляем градиент в текущей точке
                var (gradX, gradY) = ComputeGradient(currentX, currentY);

                // 2. Ищем вершину области ограничений, которая минимизирует градиентное направление
                var (optimalX, optimalY) = FindOptimalVertex(gradX, gradY);

                // 3. Вычисляем коэффициент "шага" (уменьшается с каждой итерацией)
                double learningRate = 2.0 / (step + 2);

                // 4. Обновляем текущие значения x и y по направлению к оптимальной вершине
                currentX += learningRate * (optimalX - currentX);
                currentY += learningRate * (optimalY - currentY);

                // 5. Выводим промежуточные результаты
                Console.WriteLine($"Iteration {step}: x = {currentX:F4}, y = {currentY:F4}, F(x, y) = {EvaluateObjective(currentX, currentY):F4}");

                // 6. Проверяем условие сходимости
                if (Math.Sqrt(gradX * gradX + gradY * gradY) < convergenceThreshold)
                {
                    Console.WriteLine("Convergence achieved!"); // Алгоритм сошелся
                    return;
                }
            }

            // Если достигнут лимит итераций, но сходимость не достигнута
            Console.WriteLine("Reached iteration limit without achieving convergence.");
        }

        // Точка входа в программу
        static void Main()
        {
            // Начальная точка, которая лежит в области ограничений
            double startX = 2;
            double startY = 2;

            // Количество итераций (для точного решения лучше больше, например 1000)
            int maxIterations = 1000;

            // Порог сходимости (чем меньше, тем точнее результат)
            double precision = 1e-1;

            Console.WriteLine("Starting optimization with conditional gradient descent...");
            ExecuteConditionalGradientDescent(startX, startY, maxIterations, precision);
        }
    }
}