using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        bool allowAdjacent = true;

        public Form1()
        {
            InitializeComponent();
            Task.Delay(50).ContinueWith(t => BrownianWalk2());
        }

        public void BrownianWalk2()
        {
            int width, height;
            Graphics graphics;
            Bitmap bitmap;
            InitializeBitmap(out width, out height, out graphics, out bitmap);

            var tree = new BrownianTree();
            var values = tree.Generate(width, height, (x, y) =>
            {
                graphics.FillRectangle(Brushes.White, x, y, 1, 1);
                // DrawPoints(graphics, values, x, y, newX, newY);
                pictureBox1.Invalidate();
            });
        }

        public void BrownianWalk1()
        {
            int width, height;
            Graphics graphics;
            Bitmap bitmap;
            InitializeBitmap(out width, out height, out graphics, out bitmap);

            var values = new bool[width, height];

            var halfWidth = width / 2;
            var halfHeight = height / 2;
            values[halfWidth, halfHeight] = true;
            graphics.FillRectangle(Brushes.White, halfWidth, halfHeight, 1, 1);

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

                graphics.FillRectangle(Brushes.Red, x, y, 1, 1);
                pictureBox1.Invalidate();

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
                        if (!allowAdjacent && HasMultipleAdjacent(values, x, y))
                        {
                            continue;
                        }

                        values[x, y] = true;
                        graphics.FillRectangle(Brushes.White, x, y, 1, 1);
                        // DrawPoints(graphics, values, x, y, newX, newY);
                        pictureBox1.Invalidate();

                        // If we're on the edge, we're done.  Otherwise,
                        // keep going.
                        var deltaX = x - halfWidth;
                        var deltaY = y - halfHeight;
                        var deltaXSquared = deltaX * deltaX;
                        var deltaYSquared = deltaY * deltaY;

                        if (deltaXSquared + deltaYSquared > innerRadiusSquared)
                        {
                            return;
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

        private bool HasMultipleAdjacent(bool[,] values, int x, int y)
        {
            var count =
                Value(values, x - 1, y) +
                Value(values, x + 1, y) +
                Value(values, x, y - 1) +
                Value(values, x, y + 1);
            return count > 1;
        }

        private int Value(bool[,] values, int x, int y)
        {
            return values[x, y] ? 1 : 0;
        }

        private void InitializeBitmap(out int width, out int height, 
            out Graphics graphics, out Bitmap bitmap)
        {
            var size = this.pictureBox1.Size;

            width = size.Width;
            height = size.Height;
            bitmap = new Bitmap(width, height);
            graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.Black, 0, 0, width, height);

            pictureBox1.Image = bitmap;
        }

        private static void DrawPoints(Graphics graphics, int[,] values, int x, int y, int newX, int newY)
        {
            //var xDelta = newX - x;
            //var yDelta = newY - y;

            //for (var i = 0; i < 10; i++)
            //{
                values[x, y] = 1;
                graphics.FillRectangle(Brushes.White, x, y, 1, 1);

            //    x += xDelta;
            //    y += yDelta;
            //}
        }

        private static void GetInitialStartingPoint(int width, int height, Random random, int b, out int x, out int y)
        {
            if (b == 0)
            {
                x = 0;
                y = random.Next(height);
            }
            else if (b == 1)
            {
                x = width - 1;
                y = random.Next(height);
            }
            else if (b == 2)
            {
                x = random.Next(width);
                y = 0;
            }
            else
            {
                x = random.Next(width);
                y = height - 1;
            }
        }

        private int NextDirection(Random random)
        {
            return random.Next(3) - 1;
        }

        //private static Bitmap CreateBitmap(int width, int height, bool[,] values)
        //{

        //    for (var i = 0; i < width; i++)
        //    {
        //        for (var j = 0; j < height; j++)
        //        {
        //            if (values[i, j])
        //            {
        //                graphics.FillRectangle(Brushes.Black, i, j, 1, 1);
        //            }
        //        }
        //    }

        //    return bitmap;
        //}
    }
}