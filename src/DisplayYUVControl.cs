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
        public YUVFile mFile;

        public DisplayYUVControl(AppState in_as, YUVFile cs)
        {
            mAppState = in_as;
            mFile = cs;
            Paint += new PaintEventHandler(Render);
        }

        public void SetSize()
        {
            if (mFile != null && mFile.Loaded)
            {
                Size = new System.Drawing.Size(mFile.mWidth, 
                                               mFile.mHeight);
            }
            else
            {
                Size = new System.Drawing.Size(320, 240);
            }
        }

        void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Sienna, 10, 15, 100, 100);
        }

    }
}