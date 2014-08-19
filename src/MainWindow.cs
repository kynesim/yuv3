/* MainWindow.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;

namespace yuv3
{
    public class MainWindow :  Form
    {
        AppState mAppState;
        StatusBar mStatus;
        MenuStrip mMenu;

        DisplayYUVControl mYUV;
        
        public void SetStatus(String s, bool andDialog)
        {
            mStatus.Text = s;
            if (andDialog)
            {
                MessageBox.Show(s);
            }
        }

        public MainWindow(AppState inAppState)
        {
            mAppState = inAppState;
            mAppState.SetMainWindow(this);

            mMenu = new MenuStrip();
            mMenu.Parent = this;
            ToolStripMenuItem file = new ToolStripMenuItem("&File");
            mMenu.Items.Add(file);

            ToolStripMenuItem f_open = new ToolStripMenuItem("&Open..");
            file.DropDownItems.Add(f_open);
            //f_open.DropDownItemClicked += new ToolStripItemClickedEventHandler(OnFileOpen);
            f_open.Click += new System.EventHandler(OnFileOpen);
            f_open.ShortcutKeys = Keys.Control | Keys.O;

            mYUV = new DisplayYUVControl();
            mYUV.Parent = this;
            mYUV.Anchor = AnchorStyles.None;
            mYUV.Dock = DockStyle.Fill;

            mStatus= new StatusBar();
            mStatus.Parent = this;
            
            SetStatus("Idle", false);

            Text = "YUV3";
        }



        void OnFileOpen(object sender, EventArgs e)
        {
            OpenFileDialog ofDlg = new OpenFileDialog();
            DialogResult res = ofDlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                try
                {
                    mAppState.LoadFile(ofDlg.FileName);
                }
                catch (Exception x)
                {
                    mAppState.SetStatus("Cannot open " + ofDlg.FileName + " -" + x, true);
                }
            }
        }

    }
}

/* End file */
