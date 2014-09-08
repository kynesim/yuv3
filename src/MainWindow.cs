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
        FileInterfacePanel[] mFiles;

        public DisplayYUVControl mDisplay;
        
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

            //ToolStripMenuItem f_open = new ToolStripMenuItem("&Open..");
           // file.DropDownItems.Add(f_open);
            //f_open.DropDownItemClicked += new ToolStripItemClickedEventHandler(OnFileOpen);
           // f_open.Click += new System.EventHandler(OnFileOpen);
            //f_open.ShortcutKeys = Keys.Control | Keys.O;
            
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
                mathPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

                CheckBox diffButton = new CheckBox();
                diffButton.Appearance = Appearance.Button;
                diffButton.Text = "Diff";
                mathPanel.Controls.Add(diffButton);
                controlPanel.Controls.Add(mathPanel);
            }

            topSplit.BorderStyle = BorderStyle.FixedSingle;
            topSplit.Panel1.Controls.Add(controlPanel);
            topSplit.Panel1Collapsed = false;
            topSplit.Panel2Collapsed = false;
            topSplit.Panel1MinSize = 200;
            topSplit.Panel2MinSize = 200;            
            topSplit.Dock = DockStyle.Fill;

            mDisplay = new DisplayYUVControl(inAppState, null);
            mDisplay.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            topSplit.Panel2.Controls.Add(mDisplay);
            mDisplay.BackColor = System.Drawing.Color.Blue;

            mStatus= new StatusBar();
            mStatus.Parent = this;

            SetStatus("Idle", false);

            Text = "YUV3";
        }

        void OnExit(object sender, EventArgs e)
        {
            Close();
        }


    }
}

/* End file */
