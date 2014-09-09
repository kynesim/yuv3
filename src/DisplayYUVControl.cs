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
        public Image mImage;

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
            Rectangle everywhere = new Rectangle(0, 0, this.Size.Width, this.Size.Height);
            if (mImage != null)
            {
                g.DrawImage(mImage, everywhere);
            }
            else
            {
                Brush a_brush = new System.Drawing.SolidBrush(Color.FromArgb(50, Color.Gray));
                Pen blk_pen = new Pen(new System.Drawing.SolidBrush(Color.FromArgb(50, Color.Black)));
                g.FillRectangle(a_brush, everywhere);
                g.DrawLine( blk_pen, everywhere.Left, everywhere.Bottom,
                            everywhere.Right, everywhere.Top);
                g.DrawLine( blk_pen, everywhere.Right, everywhere.Bottom,
                            everywhere.Left, everywhere.Top );
            }
        }

    }
}