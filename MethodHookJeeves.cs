namespace MethodHookJeeves
{
    class Program
    {
        static double Function(double x, double y)
        {
            return Math.Pow(x - 10, 2) + 100 * Math.Pow(y - 10, 2);
        }

        static void MethodHookJeeves(ref double x, ref double y, double step, double precision)
        {
            double fBest = Function(x, y);
            double _step = step;
            double minStep = precision / 100.0;

            do
            {
                double newX = x, newY = y;

                double f1 = Function(newX + _step, newY);
                double f2 = Function(newX - _step, newY);
                double f3 = Function(newX, newY + _step);
                double f4 = Function(newX, newY - _step);

                if (f1 < fBest)
                {
                    newX += _step;
                    fBest = f1;
                }
                else if (f2 < fBest)
                {
                    newX -= _step;
                    fBest = f2;
                }

                if (f3 < fBest)
                {
                    newY += _step;
                    fBest = f3;
                }
                else if (f4 < fBest)
                {
                    newY -= _step;
                    fBest = f4;
                }

                double fDiagonal = Function(newX + _step, newY + _step);
                if (fDiagonal < fBest)
                {
                    newX += _step;
                    newY += _step;
                    fBest = fDiagonal;
                }

                if (fBest < Function(x, y))
                {
                    x = newX;
                    y = newY;
                }
                else
                {
                    _step /= 2.0;
                }

                if (_step < minStep)
                    break;

            } while (true);

            Console.WriteLine($"Minimum value found at x = {x}, y = {y}, f(x, y) = {fBest}");
        }


        static void Main(string[] args)
        {
            double x = 1;
            double y = 1;
            double step = 0.5;
            double precision = 0.0001;

            MethodHookJeeves(ref x, ref y, step, precision);
        }
    }
}
