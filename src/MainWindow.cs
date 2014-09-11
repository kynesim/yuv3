/* MainWindow.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing;

namespace yuv3
{
    public class MainWindow :  Form, IStatusNotifier
    {
        AppState mAppState;
        StatusBar mStatus;
        MenuStrip mMenu;
        FileInterfacePanel[] mFiles;
        ToolStrip mTools;
        ToolStripTextBox mZoomBox;
        double mPixelsPerPoint;

        public DisplayYUVControl mDisplay;
        
        public DisplayYUVControl Display
        {
            get
            {
                return mDisplay;
            }
        }

        public void Warning(String s, bool dialog)
        {
            SetStatus(s, dialog);
        }

        public void Log(String s)
        {
            SetStatus(s, false);
        }

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
            mDisplay = new DisplayYUVControl(inAppState);

            Graphics g = this.CreateGraphics();
            mPixelsPerPoint = (double)g.DpiX / 72.0;
            g.Dispose();


            AutoScroll = true;

            TableLayoutPanel topFlow = new TableLayoutPanel();
            topFlow.Parent = this;
            topFlow.RowCount = 1;
            topFlow.Dock = DockStyle.Fill;

            mMenu = new MenuStrip();
            topFlow.Controls.Add(mMenu);
            ToolStripMenuItem file = new ToolStripMenuItem("&File");
            mMenu.Items.Add(file);

            //ToolStripMenuItem f_open = new ToolStripMenuItem("&Open..");
            // file.DropDownItems.Add(f_open);
            //f_open.DropDownItemClicked += new ToolStripItemClickedEventHandler(OnFileOpen);
            // f_open.Click += new System.EventHandler(OnFileOpen);
            //f_open.ShortcutKeys = Keys.Control | Keys.O;
            
            file.DropDownItems.Add(new ToolStripMenuItem("E&xit", null,
                                                         new System.EventHandler(OnExit),
                                                         Keys.Control | Keys.X));
            



            mTools= new ToolStrip();
            mTools.Anchor = AnchorStyles.Top;

            ToolStripButton zPlus = new ToolStripButton();
            zPlus.Text = "+";
            zPlus.AutoSize = true;
            mTools.Items.Add(zPlus);
            zPlus.Click += new EventHandler(OnIncreaseZoom);

            mZoomBox = new ToolStripTextBox();
            mTools.Items.Add(mZoomBox);
            mZoomBox.Text= "1";
            mZoomBox.Width = (int)(3 * mZoomBox.Font.SizeInPoints * mPixelsPerPoint);
            mZoomBox.TextChanged += new EventHandler(OnZoomChanged);
            mZoomBox.KeyDown += new KeyEventHandler(OnZoomKeyDown);

            ToolStripButton zMinus = new ToolStripButton();
            zMinus.Text = "-";
            zMinus.AutoSize = true;
            zMinus.Click += new EventHandler(OnDecreaseZoom);
            mTools.Items.Add(zMinus);

            topFlow.Controls.Add(mTools);
            

            SplitContainer topSplit = new SplitContainer();
            topSplit.Panel1.AutoScroll = true;
            topSplit.Panel2.AutoScroll = true;

            topSplit.Orientation = Orientation.Vertical;
            topSplit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | 
                AnchorStyles.Bottom;
            topSplit.AutoSize = true;

            topFlow.Controls.Add(topSplit);
            topFlow.AutoSize = true;

            TableLayoutPanel controlPanel = new TableLayoutPanel();

            controlPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top |
                AnchorStyles.Bottom;
            controlPanel.Dock = DockStyle.Fill;
            controlPanel.RowCount = 1;
            controlPanel.AutoSize = true;

            {
                TableLayoutPanel paramPanel = new TableLayoutPanel();
                paramPanel.ColumnCount = 1;
                paramPanel.AutoSize = true;
                paramPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top |
                    AnchorStyles.Bottom;

                mFiles = new FileInterfacePanel[Constants.kNumberOfChannels];
                for (int i = 0; i < Constants.kNumberOfChannels; ++i)
                {
                    mFiles[i] = new FileInterfacePanel(mAppState, i);
                    paramPanel.Controls.Add(mFiles[i]);
                }

                // Label w = new Label(); 
                // w.Text = "Width: ";
                // w.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // paramPanel.Controls.Add(w, 0, 0);
                // mWidth = new TextBox();
                // paramPanel.Controls.Add(mWidth, 1, 0);
                // mWidth.TextChanged += new EventHandler(OnWidthChanged);

                // Label h = new Label();
                // h.Text = "Height: ";
                // h.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                // paramPanel.Controls.Add(h, 0, 1);
                // mHeight = new TextBox();
                // paramPanel.Controls.Add(mHeight, 1, 1);
                // mHeight.TextChanged += new EventHandler(OnHeightChanged);

                    
                controlPanel.Controls.Add(paramPanel);
            }
            {
                TableLayoutPanel mathPanel = new TableLayoutPanel();
                mathPanel.AutoSize = true;
                mathPanel.RowCount = 1;
                mathPanel.Anchor = AnchorStyles.Top | 
                    AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

                CheckBox diffButton = new CheckBox();
                diffButton.Appearance = Appearance.Button;
                diffButton.Text = "Diff";
                mathPanel.Controls.Add(diffButton);
                controlPanel.Controls.Add(mathPanel);
                controlPanel.Dock = DockStyle.Fill;
            }

            topSplit.BorderStyle = BorderStyle.FixedSingle;
            topSplit.Panel1.Controls.Add(controlPanel);
            topSplit.Panel1Collapsed = false;
            topSplit.Panel2Collapsed = false;
            topSplit.Panel1MinSize = 200;
            topSplit.Panel2MinSize = 200;          
            topSplit.Dock = DockStyle.Fill;

            mDisplay.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom 
                | AnchorStyles.Right;
            mDisplay.AutoSize = true;
            mDisplay.Dock = DockStyle.Fill;
            topSplit.Panel2.Controls.Add(mDisplay);
            topSplit.Panel2.AutoSize = true;

//             mDisplay.BackColor = System.Drawing.Color.Blue;

            mStatus= new StatusBar();
            mStatus.Parent = this;

            Height = 600;
            Width = 1000;

            mAppState.ReplaceNotifier(this);
            SetStatus("Idle", false);

            Text = "YUV3";
        }

        public void OnZoomChanged(Object sender, EventArgs e)
        {
            mZoomBox.ForeColor = System.Drawing.Color.Blue;
        }

        public void OnZoomKeyDown(Object sender, KeyEventArgs e)
        {
            int new_zoom;
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            if (int.TryParse(mZoomBox.Text, out new_zoom))
            {
                mAppState.Zoom = new_zoom;
                mZoomBox.ForeColor = System.Drawing.Color.Black;
            }
            else 
            {
                mZoomBox.ForeColor = System.Drawing.Color.Gray;
            }
        }
        
        public void OnIncreaseZoom(Object sender, EventArgs e)
        {
            mAppState.Zoom = mAppState.Zoom + 1;
            mZoomBox.ForeColor = System.Drawing.Color.Black;
            mZoomBox.Text = mAppState.Zoom.ToString();
        }

        public void OnDecreaseZoom(Object sender, EventArgs e)
        {
            if (mAppState.Zoom > 0)
            {
                mAppState.Zoom = mAppState.Zoom -1;
                mZoomBox.ForeColor = System.Drawing.Color.Black;
                mZoomBox.Text = mAppState.Zoom.ToString();
            }
        }
        
        public void ClearRegister(int regno, int which)
        {
            mFiles[which].SetRegister(regno, false);
        }

        void OnExit(object sender, EventArgs e)
        {
            Close();
        }


    }
}

/* End file */
