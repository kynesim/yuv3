/* MainWindow.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;

namespace yuv3
{
    class MainWindow :  Form
    {
        AppState mAppState;
        StatusBar mStatus;
        MenuStrip mMenu;
        
        public void SetStatus(String s)
        {
            mStatus.Text = s;
        }

        public MainWindow(AppState inAppState)
        {
            mAppState = inAppState;

            mMenu = new MenuStrip();
            mMenu.Parent = this;
            ToolStripMenuItem file = new ToolStripMenuItem("&File");
            mMenu.Items.Add(file);

            ToolStripMenuItem f_open = new ToolStripMenuItem("Open..");
            file.DropDownItems.Add(f_open);
            

            mStatus= new StatusBar();
            mStatus.Parent = this;
            

            SetStatus("Idle");

            Text = "YUV3";
        }
    }
}

/* End file */
