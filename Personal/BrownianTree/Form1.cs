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
        public Form1()
        {
            InitializeComponent();
            Task.Delay(50).ContinueWith(t => BrownianWalk3());
        }

        public void BrownianWalk1()
        {
            int width, height;
            Graphics graphics;
            Bitmap bitmap;
            InitializeBitmap(out width, out height, out graphics, out bitmap);

            var tree = new BrownianTree1(width, height, maxTouches: 1, lineLength: 5, callback: (x, y) =>
            {
                graphics.FillRectangle(Brushes.White, x, y, 1, 1);
                // DrawPoints(graphics, values, x, y, newX, newY);
                pictureBox1.Invalidate();
            });

            tree.Generate();
        }

        public void BrownianWalk2()
        {
            int width, height;
            Graphics graphics;
            Bitmap bitmap;
            InitializeBitmap(out width, out height, out graphics, out bitmap);

            var tree = new BrownianTree2(width, height, maxTouches: 1, lineLength: 5, callback: (x, y) =>
            {
                graphics.FillRectangle(Brushes.White, x, y, 1, 1);
                // DrawPoints(graphics, values, x, y, newX, newY);
                pictureBox1.Invalidate();
            });
            tree.Generate();
        }

        public void BrownianWalk3()
        {
            int width, height;
            Graphics graphics;
            Bitmap bitmap;
            InitializeBitmap(out width, out height, out graphics, out bitmap);

            var tree = new BrownianTree3(width, height, maxTouches: 1, lineLength: 5, callback: (x, y) =>
            {
                graphics.FillRectangle(Brushes.White, x, y, 1, 1);
                // DrawPoints(graphics, values, x, y, newX, newY);
                pictureBox1.Invalidate();
            });
            tree.Generate();
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
    }
}