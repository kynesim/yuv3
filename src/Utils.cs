/* Utils.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Globalization;

namespace yuv3
{
    public class Utils
    {
        
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
