/* DisplayYUVControl.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing;

namespace yuv3
{
    class DisplayYUVControl : Control
    {
        
        public DisplayYUVControl()
        {
            Paint += new PaintEventHandler(Render);
        }

        void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Sienna, 10, 15, 100, 100);
        }

    }
}