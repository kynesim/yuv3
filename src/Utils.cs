/* Utils.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Globalization;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace yuv3
{
    public class Utils
    {
        public static unsafe Bitmap ScaleBitmap(Bitmap inmap, int zoom)
            {
                Bitmap result = new Bitmap(inmap.Width * zoom, inmap.Height * zoom, PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData inData = 
                    inmap.LockBits(new Rectangle(0, 0, inmap.Width, inmap.Height),
                                   System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                   PixelFormat.Format32bppArgb);
                System.Drawing.Imaging.BitmapData outData = 
                    result.LockBits(new Rectangle(0, 0, result.Width, result.Height),
                                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                    PixelFormat.Format32bppArgb);
                int stride_in = inData.Stride;
                int stride_out = outData.Stride;
                byte *pin = (byte *)inData.Scan0.ToPointer();
                byte *pout = (byte *)outData.Scan0.ToPointer();
                int w_in = inmap.Width;

                Parallel.For(0, inmap.Height, (j) =>
                {
                    byte *in_line = pin + (j * stride_in);
                    byte *orig_out_line = pout + ((j*zoom) * stride_out);
                    byte *out_line = orig_out_line;

                    // First line the hard way.
                    for (int i =0 ;i < w_in; ++i, in_line += 4)
                    {
                        byte a = in_line[0];
                        byte b = in_line[1];
                        byte c = in_line[2];
                        byte d = in_line[3];
                        for (int x = 0; x < zoom; ++x, out_line += 4)
                        {
                            out_line[0] =a;
                            out_line[1] = b;
                            out_line[2] = c;
                            out_line[3] = d;
                        }
                    }
                    
                    // Subsequent lines can just be copied.
                    for (int z = 1; z < zoom; ++z, out_line += stride_out)
                    {
                        for (int x = 0; x < (zoom * w_in); ++x)
                        {
                            out_line[x] = orig_out_line[x];
                        }
                    }


                });
                inmap.UnlockBits(inData);
                result.UnlockBits(outData);
                return result;
            }
                
        
        public static unsafe int clamp(int v)
            {
                if  (v < 0)
                {
                    return 0;
                }
                if (v > 255)
                {
                    return 255;
                }
                return v;
            }

        public static unsafe void YUVToRGB(byte * result, int alpha,
                                    int y, int u, int v)
        {           
            // Alpha
            int r,g ,b;
            result[3] = (byte)alpha;
            // R
            r = (int)(1.164*((float)y-16.0) + 1.596*((float)v-128.0));
            // G
            g = (int)(1.164*((float)y-16.0) - 0.813*((float)v-128.0) - 0.391*((float)u-128.0));
            // B
            b = (int)(1.164*((float)y-16.0) + 2.018*((float)u - 128.0));
            // *result = (alpha << 24) | (r << 16) | (g << 8) | b;

            r = clamp(r);
            g = clamp(g);
            b = clamp(b);

            result[2] = (byte)r;
            result[1] = (byte)g;
            result[0] = (byte)b;

            //result[0] = 0x80;
            //result[1] = 0x60;
            //result[2] = 0xFF;
            //result[3] = 0x20;
        }

        static public bool TryParseNumber(string s, out uint result)
        {
            try
            {
                if (s.StartsWith("0x") || s.StartsWith("0X"))
                {                    
                    result = uint.Parse(s.Substring(2), NumberStyles.HexNumber);
                }
                else
                {
                    result = uint.Parse(s);
                }
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }
    }
}

/* end file */
