using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    internal class BrownianTree
    {
        public bool[,] Generate(int width, int height,
            Action<int, int> callback = null)
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

                    // Hit an existing point.
                    var hitSomething = values[newX, newY];

                    if (!hitSomething)
                    {
                        var deltaX = newX - halfWidth;
                        var deltaY = newY - halfHeight;
                        var sumSquared = deltaX * deltaX + deltaY * deltaY;
                        if (sumSquared >= outerRadiusSquared)
                        {
                            hitSomething = true;
                        }
                    }
                    if (hitSomething)
                    {
                        //if (!allowAdjacent && HasMultipleAdjacent(values, x, y))
                        //{
                        //    break;
                        //}

                        values[x, y] = true;
                        if (callback != null)
                        {
                            callback(x, y);
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