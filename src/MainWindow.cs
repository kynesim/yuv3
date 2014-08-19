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

        public DisplayYUVControl mYUV;
        
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
            
            file.DropDownItems.Add(new ToolStripMenuItem("E&xit", null,
                                                         new System.EventHandler(OnExit),
                                                         Keys.Control | Keys.X));
            


            SplitContainer topSplit = new SplitContainer();
            topSplit.Orientation = Orientation.Vertical;
            topSplit.Anchor = AnchorStyles.Left;
            topSplit.Dock = DockStyle.Fill;

            topSplit.Parent = this;

            Panel controlPanel = new Panel();

            {
                Label w = new Label(); 
                w.Text = "Width: ";
                controlPanel.Controls.Add(w);
            }
            topSplit.BorderStyle = BorderStyle.FixedSingle;
            topSplit.Panel1.Controls.Add(controlPanel);
            topSplit.Panel1Collapsed = false;
            topSplit.Panel2Collapsed = false;
            topSplit.Panel1MinSize = 200;
            topSplit.Panel2MinSize = 200;
            controlPanel.BackColor = System.Drawing.Color.Red;
            
            mYUV = new DisplayYUVControl(inAppState);
            mYUV.Anchor = AnchorStyles.None;
            mYUV.Dock= DockStyle.Fill;
            topSplit.Panel2.Controls.Add(mYUV);
            mYUV.BackColor = System.Drawing.Color.Blue;

            mStatus= new StatusBar();
            mStatus.Parent = this;

            mAppState.NaturalSize();
            
            SetStatus("Idle", false);

            Text = "YUV3";
        }

        void OnExit(object sender, EventArgs e)
        {
            Close();
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
