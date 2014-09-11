/* ConsoleStatusNotifier.cs */
/* (C) Kynesim Ltd 2014 */

using System;

namespace yuv3
{
    public class ConsoleStatusNotifier : IStatusNotifier
    {
        public void Warning(string s, bool dialog)
        {
            Console.WriteLine(String.Format("Warning: {0}",s));
        }
        
        public void Log(string s)
        {
            Console.WriteLine(s);
        }

        public void MouseNotify(String s)
        {
            // On no you don't.
        }
    }
}

/* End file */
