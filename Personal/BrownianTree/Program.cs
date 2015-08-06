using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if true
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new Form1();
            Application.Run(form);
#else
            int width = 1000, height = 1000;

            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(Brushes.Black, 0, 0, width, height);
            int halfWidth = width / 2, halfHeight = height / 2;
            // var radiusSquared = halfWidth * halfWidth + halfHeight * halfHeight;
            var bestRadiusSquared = int.MaxValue;

            var values = new BrownianTree().Generate(width, height, (x, y) =>
            {
                var deltaX = halfWidth - x;
                var deltaY = halfHeight - y;
                var radiusSquared = deltaX * deltaX + deltaY * deltaY;
                if (radiusSquared < bestRadiusSquared)
                {
                    Console.WriteLine();
                    Console.WriteLine("Radius: " + Math.Sqrt(radiusSquared));
                    bestRadiusSquared = radiusSquared;
                }

                Console.Write(".");
            });

            for (var i = 0; i < width; i++)
            {
                for (var  j = 0; j < height; j++)
                {
                    if (values[i, j])
                    {
                        graphics.FillRectangle(Brushes.White, i, j, 1, 1);
                    }
                }
            }

            bitmap.Save(@"c:\temp\testing1.bmp");
#endif
        }
    }
}
