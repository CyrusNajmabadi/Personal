using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    internal class BrownianTree
    {
        private readonly int width;
        private readonly int height;
        private readonly int maxTouches;
        private readonly int lineLength;
        private readonly Action<int, int> callback;

        public BrownianTree(int width, int height, int maxTouches = 8, int lineLength = 1, Action<int, int> callback = null)
        {
            this.width = width;
            this.height = height;
            this.maxTouches = maxTouches;
            this.lineLength = lineLength;
            this.callback = callback;
        }

        public bool[,] Generate1()
        {
            var values = new bool[width, height];

            var halfWidth = width / 2;
            var halfHeight = height / 2;

            // Initialize the point in the center.
            values[halfWidth, halfHeight] = true;
            callback(halfWidth, halfHeight);

            var outerRadius = Math.Min(halfWidth, halfHeight);
            var innerRadius = (int)(outerRadius * 0.95);
            var innerRadiusSquared = innerRadius * innerRadius;

            var TwoPi = Math.PI * 2;
            var random = new Random();
            while (true)
            {
                var angle = random.NextDouble() * TwoPi;

                var x = (int)(Math.Cos(angle) * outerRadius) + halfWidth;
                var y = (int)(Math.Sin(angle) * outerRadius) + halfHeight;

                while (true)
                {
                    var newX = x + NextDirection(random);
                    var newY = y + NextDirection(random);

                    if (newX < 0 || newX >= width ||
                        newY < 0 || newY >= height)
                    {
                        // Walked off the side.  Place another random point.
                        continue;
                    }

                    if (newX == x && newY == y)
                    {
                        continue;
                    }

                    // Hit an existing point.
                    var existingValue = values[newX, newY];
                    if (existingValue)
                    {
                        if (TouchCount(values, x, y) > maxTouches || !TryPlaceLine(values, newX, newY, x, y))
                        {
                            break;
                        }

                        // If we're on the edge, we're done.  Otherwise,
                        // keep going.
                        var deltaX = x - halfWidth;
                        var deltaY = y - halfHeight;
                        var deltaXSquared = deltaX * deltaX;
                        var deltaYSquared = deltaY * deltaY;

                        if (deltaXSquared + deltaYSquared > innerRadiusSquared)
                        {
                            return values;
                        }

                        break;
                    }
                    else
                    {
                        //graphics.FillRectangle(Brushes.White, x, y, 1, 1);
                        //pictureBox1.Invalidate();
                    }

                    // Keep walking
                    x = newX;
                    y = newY;
                }
            }
        }

        private bool TryPlaceLine(bool[,] values, int fromX, int fromY, int toX, int toY)
        {
                var deltaX = toX - fromX;
                var deltaY = toY - fromY;

                for (var i = 1; i <= lineLength; i++)
                {
                    if (TouchCount(values, toX + i * deltaX, toY + i * deltaY) != 0)
                    {
                        return false;
                    }
                }

            for (var i = 0; i < lineLength; i++)
            {
                var newX = toX + i * deltaX;
                var newY = toY + i * deltaY;
                values[newX, newY] = true;
                callback(newX, newY);
            }



            return true;
        }

        private int TouchCount(bool[,] values, int x, int y)
        {
            var count =
                Value(values, x - 1, y - 1) +
                Value(values, x - 1, y) +
                Value(values, x - 1, y + 1) +

                Value(values, x, y - 1) +
                Value(values, x, y + 1) +

                Value(values, x + 1, y - 1) +
                Value(values, x + 1, y) +
                Value(values, x + 1, y + 1);
            return count;
        }

        private int Value(bool[,] values, int x, int y)
        {
            return values[x, y] ? 1 : 0;
        }

        public bool[,] Generate2()
        {
            var values = new bool[width, height];

            var halfWidth = width / 2;
            var halfHeight = height / 2;

            var outerRadius = Math.Min(halfWidth, halfHeight) * 0.95;
            var outerRadiusSquared = (int)outerRadius * outerRadius;

            var random = new Random();
            while (true)
            {
                var x = halfWidth;
                var y = halfHeight;

                while (true)
                {
                    var newX = x + NextDirection(random);
                    var newY = y + NextDirection(random);

                    if (newX < 0 || newX >= width ||
                        newY < 0 || newY >= height)
                    {
                        throw new Exception();
                        // Walked off the side.  Place another random point.
                        // continue;
                    }

                    if (newX == x && newY == y)
                    {
                        continue;
                    }


                    var deltaX = newX - halfWidth;
                    var deltaY = newY - halfHeight;
                    var sumSquared = deltaX * deltaX + deltaY * deltaY;
                    var hitCircle = sumSquared >= outerRadiusSquared;
                    if (hitCircle)
                    {
                        values[x, y] = true;
                        callback(x, y);
                        break;
                    }

                        // Hit an existing point.
                    var hitSomething = values[newX, newY];

                    if (hitSomething)
                    {
                        if (TouchCount(values, x, y) > maxTouches || TryPlaceLine(values, newX, newY, x, y))
                        {
                            break;
                        }

                        // If we're at the middle, we're done.  Otherwise,
                        // keep going.
                        if (x == halfWidth && y == halfHeight)
                        {
                            return values;
                        }

                        break;
                    }

                    // Keep walking
                    x = newX;
                    y = newY;
                }
            }
        }

        private static int NextDirection(Random random)
        {
            return random.Next(3) - 1;
        }
    }
}