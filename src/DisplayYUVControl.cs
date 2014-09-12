/* DisplayYUVControl.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace yuv3
{

    public enum FrameFieldMode
    {
        FrameField,
        MBAFF
    }



    public class LayerInfo
    {
        public Bitmap mBitmap;
        // The alpha for this image.
        public int     mAlpha;

        public LayerInfo()
        {
            mBitmap = null;
            mAlpha = 0;
        }
    }

    public class DisplayYUVControl : Control
    {
        public AppState mAppState;
        // Layers, in order.
        public LayerInfo[] mLayers;
        // If non-Null, we are displaying a math function and this is it.
        public Maths mMaths;
        public Size renderSize;
        public IStatusNotifier mNotifier;
        public bool mMBGrid, mBlockGrid, mPixelGrid;
        public FrameFieldMode mFF;
        public Color mGridColor;

        public MathsOperation MathsOperation
        {
            get
            {
                return mMaths.Operation;
            }
            set
            {
                mMaths.Operation = value;
            }
        }

        public DisplayYUVControl(AppState in_as, IStatusNotifier inNotifier)
        {
            int i;
            mAppState = in_as;
            mNotifier = inNotifier;
            mLayers = new LayerInfo[Constants.kNumberOfChannels];
            mMBGrid = false;
            mBlockGrid = false;
            mPixelGrid = false;
            
            mFF = FrameFieldMode.FrameField;
            for (i =0 ;i < Constants.kNumberOfChannels; ++i)
            {
                mLayers[i] = new LayerInfo();
            }
            mMaths = new Maths(in_as, inNotifier);
            mGridColor = Color.FromArgb(255, Color.Black);
            Paint += new PaintEventHandler(Render);
            MouseLeave += new EventHandler(OnMouseLeave);
            MouseMove += new MouseEventHandler(OnMouseMove);
            Size = new Size(320, 240);
        }

        public void SetGridColor(Color c)
        {
            mGridColor = c;
            Refresh();
        }

        public void SetGridMode(bool mb, bool block, bool pixel)
        {
            mMBGrid = mb;
            mBlockGrid = block;
            mPixelGrid = pixel;
            mNotifier.MouseNotify("");
            Refresh();
        }

        public void SetFFMode(FrameFieldMode f)
        {
            mFF = f;
            mNotifier.MouseNotify("");
            Refresh();
        }

        public string MouseCoords(int x, int y)
        {
            int mb_l = (mFF == FrameFieldMode.MBAFF ? 8 : 16);
            int mb_x = (x/16);
            int mb_y =  (y/mb_l);
            int rem_x = (x- (mb_x * 16));
            int rem_y = (y- (mb_y * mb_l));
            return String.Format("({0} x {1}) MB [{2}, {3}] + <{4}, {5}> ",
                                 x,y, 
                                 mb_x, mb_y,
                                 rem_x, rem_y);
        }

        public void UpdateMaths()
        {
            mMaths.Update();
            ImagesChanged();
        }

        public void UpdateLayer(int layer, YUVFile aFile)
        {
            /* Update mImage[layer] */
            int nr = 0, alpha = 0;
            LayerInfo my_info = mLayers[layer];
            if (aFile == null)
            {
                my_info.mBitmap = null;
            }
            else
            {
                // Let's assume that we are going to succeed in activating this layer.
                int i;
                for (i =0 ; i < mLayers.Length; ++i)
                {
                    if (mLayers[i].mBitmap != null || i == layer)
                    {
                        ++nr;
                    }
                }
                alpha = 255/nr;
                RenderToBitmap(mLayers[layer], aFile, alpha);
            }

            // Now, what is the new alpha?
            nr = 0;
            for (int i =0 ;i < mLayers.Length; ++i)
            {
                if (mLayers[i].mBitmap != null)
                {
                    ++nr;
                }
            }
            if (nr > 0)
            {
                alpha = 255/nr;
                for (int i = 0; i < mLayers.Length; ++i)
                {
                    if (mLayers[i].mBitmap != null && mLayers[i].mAlpha != alpha)
                    {
                        SetAlpha(mLayers[i], alpha);
                        mLayers[i].mAlpha = alpha;
                    }
                }
            }
            this.ImagesChanged();
        }

        public void RenderToBitmap(LayerInfo result, YUVFile aFile, int alpha)
        {
            bool ok;
            if (aFile.Width < 1 || aFile.Height < 1)
            {
                // None of that or any of the other.
                Console.WriteLine("+++ ERROR. OUT OF CHEESE. REDO FROM START.");
                result.mBitmap = null;
                return;
            }
            if (result.mBitmap == null ||
                result.mBitmap.Width != aFile.mWidth ||
                result.mBitmap.Height != aFile.mHeight)
            {
                result.mBitmap = new Bitmap(aFile.mWidth, aFile.mHeight, PixelFormat.Format32bppArgb);
            }
            System.Drawing.Imaging.BitmapData someData =
                result.mBitmap.LockBits(new Rectangle(0, 0, result.mBitmap.Width, result.mBitmap.Height),
                                        System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                        result.mBitmap.PixelFormat);
            ok = aFile.ToRGB32(someData, alpha);
            result.mBitmap.UnlockBits(someData);
            if (!ok)
            {
                result.mBitmap = null;
            }
        }

        public unsafe void SetAlpha(LayerInfo result, int alpha)
        {
            if (result.mBitmap == null) return;
            System.Drawing.Imaging.BitmapData someData =
                result.mBitmap.LockBits(new Rectangle(0, 0, result.mBitmap.Width, result.mBitmap.Height),
                                        System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                        result.mBitmap.PixelFormat);
            byte *a_buffer = (byte *)someData.Scan0.ToPointer();
            int stride = someData.Stride;
            int w = result.mBitmap.Width;
            Parallel.For(0, result.mBitmap.Height, (j) =>
            {
                byte *ptr = a_buffer + (j *stride) + 3;
                for (int i = 0; i < w; ++i, ptr += 4)
                {
                    *ptr = (byte)alpha;
                }
            });
            result.mBitmap.UnlockBits(someData);
        }


        public void ImagesChanged()
       {
            /* Work out the new size for this control */
            // We never claim to be any smaller than this.
            int max_w = 320, max_h = 240;
            double z = mAppState.Zoom;
            mMaths.Update();
            if (mMaths.Bitmap != null)
            {
                max_w = (int)(mMaths.Bitmap.Width * z);
                max_h = (int)(mMaths.Bitmap.Height * z);
                mAppState.SetIsMaths(true);
            }
            else
            {                
                mAppState.SetIsMaths(false);
                for (int i =0 ;i < mLayers.Length; ++i)
                {
                    if (mLayers[i].mBitmap != null)
                    {
                        int w = (int)(mLayers[i].mBitmap.Width * z);
                        int h = (int)(mLayers[i].mBitmap.Height * z);
                        if (w > max_w)
                        {
                            max_w =w;
                        }
                        if (h > max_h)
                        {
                        max_h = h;
                        }
                    }
                }
            }
            this.renderSize = new Size(max_w, max_h);
            this.Size = this.renderSize;
            this.Refresh();
        }

        public override Size GetPreferredSize(Size proposed)
        {
            return this.renderSize;
        }

        void OnMouseLeave(Object sender, EventArgs e)
        {
            mNotifier.MouseNotify("");
        }

        void OnMouseMove(Object sender, MouseEventArgs e)
        {
            if (mAppState != null && mAppState.Zoom > 0)
            {
                int pix_x = (int)(e.X / mAppState.Zoom);
                int pix_y = (int)(e.Y / mAppState.Zoom);
                int m = mAppState.ToMeasure;
                if (m >= 0 && pix_x >= 0 && pix_y >= 0)
                {
                    if (mMaths.Bitmap != null)
                    {
                        mNotifier.MouseNotify(MouseCoords(pix_x, pix_y) + mMaths.MouseQuery(pix_x, pix_y));
                    }
                    else
                    {
                        Bitmap bm = mLayers[m].mBitmap;
                        if (bm != null && pix_x < bm.Width && pix_y < bm.Height)
                        {
                            Color rgb = bm.GetPixel(pix_x, pix_y);
                            int cy, cu, cv;
                            mAppState.QueryYUV(m,pix_x,pix_y, out cy, out cu, out cv);
                            mNotifier.MouseNotify(MouseCoords(pix_x, pix_y) + 
                                                  String.Format("R = {2} G = {3} B = {4} Y = {5} U = {6} V = {7}",
                                                                pix_x, pix_y, 
                                                                rgb.R, rgb.G, rgb.B, cy, cu, cv));
                        }
                    }
                }
            }
        }

        void DrawGrid(PaintEventArgs e, Pen p, double zoom, int x_grid, int y_grid)
        {
            double xp = (x_grid * zoom);
            double yp = (y_grid * zoom);
            Rectangle to_paint = e.ClipRectangle;
            Graphics g = e.Graphics;
            double top_p = Math.Round(to_paint.Top / yp) * yp;
            double left_p = Math.Round(to_paint.Left / xp) * xp;
            // H lines.
            for (double j = top_p; j < (to_paint.Bottom + yp); j += yp)
            {
                g.DrawLine(p, to_paint.Left , (int)j, to_paint.Right, (int)j);
            }
            // V lines
            for (double i = left_p; i < (to_paint.Right + xp); i += xp)
            {
                g.DrawLine(p, (int)i, to_paint.Top, (int)i, to_paint.Bottom);
            }
        }

        void DrawEmptyPage(PaintEventArgs e, Rectangle everywhere)
        {
            Brush a_brush = new System.Drawing.SolidBrush(Color.FromArgb(50, Color.Gray));
            Pen blk_pen = new Pen(new System.Drawing.SolidBrush(Color.FromArgb(50, Color.Black)));
            Graphics g = e.Graphics;
            g.FillRectangle(a_brush, everywhere);
            g.DrawLine( blk_pen, everywhere.Left, everywhere.Bottom,
                        everywhere.Right, everywhere.Top);
            g.DrawLine( blk_pen, everywhere.Right, everywhere.Bottom,
                        everywhere.Left, everywhere.Top );
        }

        void Render(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            bool any = false;
            double zoom = mAppState.Zoom;
            Rectangle everywhere = new Rectangle(0, 0, (int)(zoom * this.renderSize.Width), 
                                                 (int)(zoom * this.renderSize.Height));

            if (zoom <= 0)
            {
                DrawEmptyPage(e, e.ClipRectangle);
                return;
            }
            // Speed up scaling -we do a lot of it.
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            

            Rectangle clip = e.ClipRectangle;
            Rectangle src_it = new Rectangle((int)(clip.Left / zoom),
                                             (int)(clip.Top / zoom),
                                             (int)((clip.Width + zoom) /zoom),
                                             (int)((clip.Height + zoom) /zoom));
                                             


            Bitmap maths = mMaths.Bitmap;
            if (maths != null)
            {
                if (Constants.kFastDrawing)
                {
                    g.DrawImage(maths, clip, src_it, GraphicsUnit.Pixel);                    
                }
                else
                {
                    Rectangle to_draw = new Rectangle(0, 0,
                                                      (int)(maths.Width * zoom), 
                                                      (int)(maths.Height * zoom));
                    g.DrawImage(maths, to_draw);
                }
            }
            else
            {
                for (int i = 0; i < Constants.kNumberOfChannels; ++i)
                {
                    if (mLayers[i].mBitmap != null)
                    {
                        if (Constants.kFastDrawing)
                        {
                            g.DrawImage(mLayers[i].mBitmap, clip, src_it, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            Bitmap a_map = mLayers[i].mBitmap;
                            Rectangle to_draw = new Rectangle(0, 0, 
                                                              (int)(a_map.Width * zoom), 
                                                              (int)(a_map.Height * zoom));
                            
                             g.DrawImage(mLayers[i].mBitmap, to_draw);
                        }
                        any = true;
                    }
                }

                if (!any)
                {
                    DrawEmptyPage(e, everywhere);
                    return;
                }
            }

            if (mMBGrid || mBlockGrid || mPixelGrid && zoom > 0)
            {
                Pen line_pen = new Pen(new System.Drawing.SolidBrush(mGridColor));
                if (mMBGrid)
                {
                    DrawGrid(e, line_pen, zoom, 16,
                             (mFF == FrameFieldMode.MBAFF ? 8 : 16));
                }
                if (mBlockGrid)
                {
                    line_pen.DashStyle = DashStyle.Dash;
                    DrawGrid(e, line_pen, zoom, 8, 8);
                }
                if (mPixelGrid && zoom > 4.0)
                {
                    line_pen.DashStyle = DashStyle.Dot;
                    DrawGrid(e, line_pen, zoom, 1, 1);
                }
            }

        }

    }
}