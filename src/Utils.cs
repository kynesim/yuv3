/* Utils.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Globalization;

namespace yuv3
{
    public class Utils
    {
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
