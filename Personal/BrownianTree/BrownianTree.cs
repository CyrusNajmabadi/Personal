using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    internal abstract class AbstractBrownianTree
    {
        protected const int DefaultMaxTouches = 8;
        protected const int DefaultLineLength = 1;
        protected const double TwoPi = Math.PI * 2;

        protected readonly int width;
        protected readonly int height;
        protected readonly int maxTouches;
        protected readonly int lineLength;
        protected readonly Action<int, int> callback;
        protected readonly Random random;
        protected readonly bool[,] values;

        protected AbstractBrownianTree(int width, int height, int maxTouches, int lineLength, Action<int, int> callback, int? seed)
        {
            this.random = seed == null ? new Random() : new Random(seed.Value);
            this.width = width;
            this.height = height;
            this.maxTouches = maxTouches;
            this.lineLength = lineLength;
            this.callback = callback;
            this.values = new bool[width, height];
        }

        public bool[,] Generate()
        {
            while (true)
            {
                int x, y;
                GetStartingPoint(out x, out y);

                while (true)
                {
                    int newX, newY;
                    MovePoint(x, y, out newX, out newY);

                    if (newX == x && newY == y)
                    {
                        continue;
                    }

                    if (newX < 0 || newX >= width ||
                        newY < 0 || newY >= height)
                    {
                        break;
                    }

                    // Hit an existing point.
                    var existingValue = values[newX, newY];
                    if (existingValue)
                    {
                        if (TryPlacePoints(newX, newY, x, y))
                        {
                            if (Stop(x, y))
                            {
                                return values;
                            }
                        }

                        break;
                    }

                    if (HitEdge(newX, newY))
                    {
                        // Walked out of bounds.  Start over.
                        break;
                    }

                    // Keep walking
                    x = newX;
                    y = newY;
                }
            }
        }

        protected abstract void GetStartingPoint(out int x, out int y);
        protected abstract void MovePoint(int x, int y, out int newX, out int newY);
        protected abstract bool HitEdge(int newX, int newY);
        protected abstract bool TryPlacePoints(int newX, int newY, int x, int y);
        protected abstract bool Stop(int x, int y);

        protected bool TryPlaceLine(int fromX, int fromY, int toX, int toY)
        {
            var deltaX = toX - fromX;
            var deltaY = toY - fromY;

            for (var i = 1; i <= lineLength; i++)
            {
                if (TouchCount(toX + i * deltaX, toY + i * deltaY) != 0)
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

        protected int TouchCount(int x, int y)
        {
            var count =
                Value(x - 1, y - 1) +
                Value(x - 1, y) +
                Value(x - 1, y + 1) +

                Value(x, y - 1) +
                Value(x, y + 1) +

                Value(x + 1, y - 1) +
                Value(x + 1, y) +
                Value(x + 1, y + 1);
            return count;
        }

        private int Value(int x, int y)
        {
            if (x >= 0 && x < width &&
                y >= 0 && y < height &&
                values[x, y])
            {
                return 1;
            }

            return 0;
        }

        protected int NextDirection()
        {
            return random.Next(3) - 1;
        }
    }

    internal class BrownianTree1 : AbstractBrownianTree
    {
        private readonly int halfWidth;
        private readonly int halfHeight;
        private readonly int innerRadiusSquared;
        private readonly int outerRadius;

        public BrownianTree1(int width, int height, int maxTouches = DefaultMaxTouches, int lineLength = DefaultLineLength, Action<int, int> callback = null, int? seed = null)
            : base(width, height, maxTouches, lineLength, callback, seed)
        {
            this.halfWidth = width / 2;
            this.halfHeight = height / 2;

            // Initialize the point in the center.
            values[halfWidth, halfHeight] = true;
            callback(halfWidth, halfHeight);

            this.outerRadius = Math.Min(halfWidth, halfHeight);
            var innerRadius = (int)(outerRadius * 0.95);
            this.innerRadiusSquared = innerRadius * innerRadius;
        }

        protected override void GetStartingPoint(out int x, out int y)
        {
            var angle = random.NextDouble() * TwoPi;

            x = (int)(Math.Cos(angle) * outerRadius) + halfWidth;
            y = (int)(Math.Sin(angle) * outerRadius) + halfHeight;
        }

        protected override void MovePoint(int x, int y, out int newX, out int newY)
        {
            newX = x + NextDirection();
            newY = y + NextDirection();
        }

        protected override bool HitEdge(int newX, int newY)
        {
            return false;
        }

        protected override bool TryPlacePoints(int newX, int newY, int x, int y)
        {
            if (TouchCount(x, y) > maxTouches || !TryPlaceLine(newX, newY, x, y))
            {
                return false;
            }

            return true;
        }

        protected override bool Stop(int x, int y)
        {
            // If we're on the edge, we're done.  Otherwise,
            // keep going.
            var deltaX = x - halfWidth;
            var deltaY = y - halfHeight;
            var deltaXSquared = deltaX * deltaX;
            var deltaYSquared = deltaY * deltaY;

            if (deltaXSquared + deltaYSquared > innerRadiusSquared)
            {
                return true;
            }

            return false;
        }
    }

    internal class BrownianTree2 : AbstractBrownianTree
    {
        private readonly int halfWidth;
        private readonly int halfHeight;
        private readonly double outerRadiusSquared;

        public BrownianTree2(int width, int height, int maxTouches = DefaultMaxTouches, int lineLength = DefaultLineLength, Action<int, int> callback = null, int? seed = null)
            : base(width, height, maxTouches, lineLength, callback, seed)
        {
            this.halfWidth = width / 2;
            this.halfHeight = height / 2;

            var outerRadius = Math.Min(halfWidth, halfHeight) * 0.95;
            this.outerRadiusSquared = (int)outerRadius * outerRadius;
        }

        protected override void GetStartingPoint(out int x, out int y)
        {
            x = halfWidth;
            y = halfHeight;
        }

        protected override void MovePoint(int x, int y, out int newX, out int newY)
        {
            newX = x + NextDirection();
            newY = y + NextDirection();

            //if (newX < 0 || newX >= width ||
            //    newY < 0 || newY >= height)
            //{
            //    throw new Exception();
            //    // Walked off the side.  Place another random point.
            //    // continue;
            //}
        }

        protected override bool HitEdge(int newX, int newY)
        {
            var deltaX = newX - halfWidth;
            var deltaY = newY - halfHeight;
            var sumSquared = deltaX * deltaX + deltaY * deltaY;
            var hitCircle = sumSquared >= outerRadiusSquared;
            if (hitCircle)
            {
                values[newX, newY] = true;
                callback(newX, newY);
                return true;
            }

            return false;
        }

        protected override bool TryPlacePoints(int newX, int newY, int x, int y)
        {
            if (TouchCount(x, y) > maxTouches || TryPlaceLine(newX, newY, x, y))
            {
                return false;
            }

            return true;
        }

        protected override bool Stop(int x, int y)
        {

            // If we're at the middle, we're done.  Otherwise,
            // keep going.
            if (x == halfWidth && y == halfHeight)
            {
                return true;
            }

            return false;
        }
    }

    internal class BrownianTree3 : BrownianTree2
    {
        public BrownianTree3(int width, int height, int maxTouches = DefaultMaxTouches, int lineLength = DefaultLineLength, Action<int, int> callback = null, int? seed = default(int?)) 
            : base(width, height, maxTouches, lineLength, callback, seed)
        {
        }

        protected override void GetStartingPoint(out int x, out int y)
        {
            var r = random.NextDouble();
            var theta = random.NextDouble() * TwoPi;

            var sqrtR = Math.Sqrt(r);
            var unitX = (sqrtR * Math.Cos(theta) + 1) / 2;
            var unitY = (sqrtR * Math.Sin(theta) + 1) / 2;

            x = (int)(width * unitX);
            y = (int)(height * unitY);
        }
    }
}