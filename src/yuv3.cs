/* yuv3.cs */
/* (C) Kynesim Ltd 2014 */

/** @file
 *
 *  A YUV viewer, just like YUV2 used to be.
 *
 * @author Richard Watts <rrw@kynesim.co.uk>
 * @date   2014-08-19
 */

using System;
using System.Windows.Forms;

public class yuv3main : Form
{
    [STAThread]
    static public void Main(String[] s)
    {
        yuv3.AppState state = new yuv3.AppState();
        Application.Run(new yuv3.MainWindow(state, s));
    }
}

/* End file */
