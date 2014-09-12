/* Maths.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Globalization;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading.Tasks;

namespace yuv3
{
    public enum MathsOperation
    {
        SubtractAB,
        None
    }

    public class Maths
    {
        const int ResultIdx = 0;
        const int OpAIdx = 1;
        const int OpBIdx = 2;
        const int kPixelPlanes = 3;

        AppState mAppState;
        /* Result = 0, OpA = 1, OpB = 2 .. , Y, U, V */
        int[,] mPixels;
        int mWidth, mHeight;
        // Number of bits of significance.
        int mMaxBits;
        IStatusNotifier mNotifier;
        Bitmap mBitmap;
        MathsOperation mOp;

        public MathsOperation Operation
        {
            get
            {
                return mOp;
            }
            set
            {
                if (value != mOp)
                {
                    Console.WriteLine("Op!");
                    mOp = value;
                    Update();
                }
            }
        }

        public bool Valid
        {
            get
            {
                return (mPixels != null);
            }
        }

        public Bitmap Bitmap
        {
            get
            {
                if (mBitmap == null && mOp != MathsOperation.None) 
                {
                    EnsureBitmap();
                }
                return mBitmap;
            }
        }

        public Maths(AppState app, IStatusNotifier inNotifier)
        {
            mPixels = null;
            mWidth = mHeight = 0;
            mNotifier = inNotifier;
            mOp = MathsOperation.None;
            mAppState = app;
        }

        public void Update()
        {
            /* @todo Could be done much more cheaply by spotting redundant ops */
            Clear();
            Operate();
        }


        public void Operate()
        {
            switch (mOp)
            {
            case MathsOperation.None:
                Clear();
                break;
            case MathsOperation.SubtractAB:
                Subtract(mAppState.FileForRegister(Constants.RegisterA),
                         mAppState.FileForRegister(Constants.RegisterB));
                break;
            }
            EnsureBitmap();
        }

        public void Clear()
        {
            mPixels = null;
            mWidth = 0; mHeight = 0;
            mBitmap = null;
        }

        public  int PixOffset(int x, int y)
        {
            return 3*( (mWidth * y) + x);
        }

        public string MouseQuery(int x, int y)
        {
            int po = PixOffset(x,y);
            if (x > mWidth ||  y > mHeight)
            {
                return String.Format("({0}, {1}) Out of range", x, y);
            }

            switch (mOp)
            {
            case MathsOperation.None:
                return String.Format("(maths: no operation @ {0}, {1}", x,y );
            case MathsOperation.SubtractAB:
            {
                return String.Format("({0}, {1}) -> Y {2} U {3} V {4} [ {5}-{6}, {7}-{8}, {9}-{10} ]",
                                     x,y,
                                     mPixels[ResultIdx,po], mPixels[ResultIdx,po+1], mPixels[ResultIdx,po+2],
                                     mPixels[OpAIdx,po], mPixels[OpBIdx,po], 
                                     mPixels[OpAIdx,po+1], mPixels[OpBIdx,po+1],
                                     mPixels[OpAIdx,po+2],
                                     mPixels[OpBIdx,po+2]);
            }
            default:
                return "(bad operation)";
            }
        }

        public unsafe void Subtract(YUVFile a, YUVFile b)
        {
            if (a == null || b == null)
            {
                mNotifier.Warning("Either A or B is empty - cannot subtract", false);
                Clear();
                return;
            }

            Console.WriteLine("Hit subtract!");
            int min_x = (a.Width < b.Width ? a.Width : b.Width);
            int min_y = (a.Height < b.Height ? a.Height : b.Height);
            mOp = MathsOperation.SubtractAB;
            mWidth = min_x; mHeight = min_y;
            mMaxBits = (a.PixelBits > b.PixelBits ? a.PixelBits : b.PixelBits);
            mBitmap = null; // No bitmap any more.
            mPixels = new int[kPixelPlanes, mWidth * mHeight * 3];
            int[,] p = mPixels;

            /* Do this in parallel .. */
            Parallel.For(0, min_y, (j) =>
            {
                int line = (3 *j * min_x);
                for (int i = 0; i < min_x; ++i, line += 3)
                {
                    a.QueryYUV(i,j, out p[OpAIdx, line], out p[OpAIdx, line+1], out p[OpAIdx, line+2]);
                    b.QueryYUV(i,j, out p[OpBIdx, line], out p[OpBIdx, line+1], out p[OpBIdx, line+2]);
                    p[ResultIdx, line] = p[OpAIdx, line] - p[OpBIdx, line];
                    p[ResultIdx, line+1] = p[OpAIdx, line+1] - p[OpBIdx, line+1];
                    p[ResultIdx, line+2] = p[OpAIdx, line+2] - p[OpBIdx, line+2];
                }
            });
        }
            
        unsafe void EnsureBitmap()
        {
            Console.Write("EnsureBitmap!");
            if (mBitmap == null && mPixels != null)
            {
                Console.Write("EnsureBitmap[2]!");
                Bitmap result = new Bitmap(mWidth, mHeight, PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData someData = 
                    result.LockBits(new Rectangle(0,0, mWidth, mHeight),
                                            System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                    result.PixelFormat);
                int shift = mMaxBits - 8;
                int stride = someData.Stride;
                byte *out_pic = (byte *)someData.Scan0.ToPointer();
                int[,] pixels = mPixels;
                

                Parallel.For(0, mHeight, (j) =>
                {
                    /* Translate our values to pixels as best we can */
                    byte *out_line = out_pic + (j * stride);
                    int in_line = (3 * j * mWidth);
                    for (int i = 0; i < mWidth; ++i, out_line += 4, in_line += 3)
                    {
                        Utils.YUVToRGB(out_line, 255, 
                                       (pixels[ResultIdx, in_line] >> shift),
                                       (pixels[ResultIdx, in_line + 1] >> shift),
                                       (pixels[ResultIdx, in_line + 2] >> shift));
                    }
                });

                result.UnlockBits(someData);
                mBitmap = result;
            }
        }
    }
}
    
/* End file */
