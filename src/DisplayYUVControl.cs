/* DisplayYUVControl.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing;

namespace yuv3
{
    public class DisplayYUVControl : Control
    {
        public AppState mAppState;
        public Bitmap[] mImage;
        public Bitmap mMath;

        public DisplayYUVControl(AppState in_as)
        {
            mAppState = in_as;
            mImage = new Bitmap[Constants.kNumberOfChannels];
            mMath = null;
            Paint += new PaintEventHandler(Render);
        }

        public void UpdateLayer(int layer, YUVFile aFile, int alpha)
        {
            /* Update mImage[layer] */
            bool ok;
            Bitmap myBitmap = new Bitmap(aFile.mWidth, aFile.mHeight, PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData someData =
                myBitmap.LockBits(new Rectangle(0, 0, myBitmap.Width, myBitmap.Height),
                                  System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                  myBitmap.PixelFormat);
            ok = aFile.ToRGB32(someData, alpha);
            myBitmap.UnlockBits(someData);
            if (ok)
            {
                Console.WriteLine("Data!\n");
                mImage[layer] = myBitmap;
            }
            else
            {
                mImage[layer]= null;
            }
            this.Refresh();
        }

        void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle everywhere = new Rectangle(0, 0, this.Size.Width, this.Size.Height);
            bool any = false;

            // Speed up scaling -we do a lot of it.
            // g.InterpolationMode = InterpolationMode.NearestNeighbour;

            for (int i = 0; i < Constants.kNumberOfChannels; ++i)
            {
                if (mImage[i] != null)
                {
                    Console.WriteLine(String.Format("Paint {0}", i));
                    g.DrawImage(mImage[i], everywhere);
                    any = true;
                }
            }

            if (!any)
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