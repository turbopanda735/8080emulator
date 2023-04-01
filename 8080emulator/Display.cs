using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _8080emulator
{
    public class Display
    {
        private readonly int scale;
        private readonly Bitmap bitmap;
        private readonly Graphics graphics;
        private readonly Brush onBrush;
        private readonly Brush offBrush;

        public Display(int width, int height, int scale)
        {
            this.scale = scale;
            bitmap = new Bitmap(width * scale, height * scale);
            graphics = Graphics.FromImage(bitmap);
            onBrush = new SolidBrush(Color.White);
            offBrush = new SolidBrush(Color.Black);
        }

        public void Clear()
        {
            graphics.Clear(Color.Black);
        }
        public void Draw(byte[] displayData)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    int index = y * 64 + x;
                    Brush brush = (displayData[index] == 1) ? onBrush : offBrush;
                    graphics.FillRectangle(brush, x * scale, y * scale, scale, scale);
                }
            }
        }
        public void Render(Graphics target)
        {
            target.DrawImage(bitmap, 0, 0);
        }
    }
}
