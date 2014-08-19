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

            TableLayoutPanel topFlow = new TableLayoutPanel();
            topFlow.Parent = this;
            topFlow.RowCount = 1;
            topFlow.Dock = DockStyle.Fill;

            mMenu = new MenuStrip();
            topFlow.Controls.Add(mMenu);
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

            topFlow.Controls.Add(topSplit);

            TableLayoutPanel controlPanel = new TableLayoutPanel();

            controlPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top |
                AnchorStyles.Bottom;
            controlPanel.Dock = DockStyle.Fill;
            controlPanel.RowCount = 1;

            {
                TableLayoutPanel paramPanel = new TableLayoutPanel();
                paramPanel.RowCount = 2;
                Label w = new Label(); 
                w.Text = "Width: ";
                w.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                paramPanel.Controls.Add(w, 0, 0);
                TextBox wval = new TextBox();
                paramPanel.Controls.Add(wval, 1, 0);
                paramPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                controlPanel.Controls.Add(paramPanel);
            }

            topSplit.BorderStyle = BorderStyle.FixedSingle;
            topSplit.Panel1.Controls.Add(controlPanel);
            topSplit.Panel1Collapsed = false;
            topSplit.Panel2Collapsed = false;
            topSplit.Panel1MinSize = 200;
            topSplit.Panel2MinSize = 200;            
            topSplit.Dock = DockStyle.Fill;

            mYUV = new DisplayYUVControl(inAppState);
            mYUV.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
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
