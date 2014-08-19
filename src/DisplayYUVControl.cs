/* DisplayYUVControl.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing;

namespace yuv3
{
    public class DisplayYUVControl : Control
    {
        public AppState mAppState;

        public DisplayYUVControl(AppState in_as)
        {
            mAppState = in_as;
            Paint += new PaintEventHandler(Render);
        }

        public void SetSize()
        {
            Size = new System.Drawing.Size(mAppState.mYUVFile.mWidth, 
                                       mAppState.mYUVFile.mHeight);

        }

        void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Sienna, 10, 15, 100, 100);
        }

    }
}