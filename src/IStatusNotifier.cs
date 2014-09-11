/* StatusNotifier.cs */
/* (C) Kynesim Ltd 2014 */

using System;

namespace yuv3
{
    public interface IStatusNotifier
    {
        void Warning(string s, bool dialog);
        void Log(String s);
        void MouseNotify(String s);
   }
}