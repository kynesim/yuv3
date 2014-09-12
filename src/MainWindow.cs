/* MainWindow.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace yuv3
{
    public class MainWindow :  Form, IStatusNotifier
    {
        AppState mAppState;
        MenuStrip mMenu;
        FileInterfacePanel[] mFiles;
        ToolStrip mTools;
        ToolStripTextBox mZoomBox;
        ToolStripStatusLabel mStatus;
        double mPixelsPerPoint;
        ToolStripButton mSubtractButton;
        ToolStripComboBox mTrackerCombo;
        ToolStripComboBox mFFCombo;
        ToolStripButton mMBGrid, mBlockGrid, mPixelGrid;
        ToolStripButton mGridColor;
        ToolStripLabel mGridColorLabel;
        ToolStripButton mSoftScaling;
        Assembly _assembly;
        Stream _bgimageStream;

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

        public void MouseNotify(String s)
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


        public MainWindow(AppState inAppState, String[] filesToLoad)
        {
            // Faff with reflection.
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                _bgimageStream = _assembly.GetManifestResourceStream("MainBackground.png");
            }
            catch
            {
                MessageBox.Show("Cannot load resources");
            }
               

            mAppState = inAppState;
            mAppState.SetMainWindow(this);
            mDisplay = new DisplayYUVControl(inAppState, this);

            Graphics g = this.CreateGraphics();
            mPixelsPerPoint = (double)g.DpiX / 72.0;
            g.Dispose();

            this.BackColor = System.Drawing.Color.White;

            FlowLayoutPanel topFlow = new FlowLayoutPanel();
            topFlow.FlowDirection = FlowDirection.TopDown;
            topFlow.Dock = DockStyle.Fill;
            topFlow.Anchor = AnchorStyles.Left |
                AnchorStyles.Bottom | AnchorStyles.Right;
            topFlow.AutoSize = true;

            mMenu = new MenuStrip();

            // topFlow.Controls.Add(mMenu);
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
            

            this.MainMenuStrip = mMenu;
            topFlow.BackColor = System.Drawing.Color.Blue;



            mTools= new ToolStrip();

            ToolStripLabel l = new ToolStripLabel();
            l.Text = "Zoom";
            l.AutoSize = true;
            mTools.Items.Add(l);

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
           
            mSoftScaling = new ToolStripButton();
            mSoftScaling.Text = "SoftScale";
            mSoftScaling.AutoSize = true;
            mSoftScaling.CheckOnClick = true;
            mSoftScaling.CheckedChanged += new EventHandler(OnSoftScaleChanged);;
            mTools.Items.Add(mSoftScaling);

            mTools.Items.Add(new ToolStripSeparator());

            l = new ToolStripLabel();
            l.AutoSize = true;
            l.Text = "Track";
            mTools.Items.Add(l);

            mTrackerCombo = new ToolStripComboBox();

            {
                string[] items = new string[Constants.kNumberOfChannels];

                for (int i =0 ;i < items.Length; ++i)
                {
                    items[i] = String.Format("File#{0}", i);                
                }

                mTrackerCombo.Items.AddRange(items);
            }
            mTrackerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mTrackerCombo.SelectedIndexChanged +=
                new EventHandler(TrackerIndexChanged);

            mTools.Items.Add(mTrackerCombo);

            mTools.Items.Add(new ToolStripSeparator());

            mSubtractButton = new ToolStripButton();
            mSubtractButton.Text = "A-B";
            mSubtractButton.AutoSize = true;
            mSubtractButton.CheckOnClick = true;
            mSubtractButton.CheckedChanged += new EventHandler(OnSubtractChanged);
            mTools.Items.Add(mSubtractButton);
            
            mTools.Items.Add(new ToolStripSeparator());
            
            mMBGrid = new ToolStripButton();
            mMBGrid.CheckOnClick = true;
            mMBGrid.Text = "MB";
            mMBGrid.AutoSize = true;
            mMBGrid.CheckedChanged += new EventHandler(GridButtonsChanged);
            mTools.Items.Add(mMBGrid);

            mBlockGrid = new ToolStripButton();
            mBlockGrid.CheckOnClick = true;
            mBlockGrid.Text = "Block";
            mBlockGrid.AutoSize = true;
            mBlockGrid.CheckedChanged += new EventHandler(GridButtonsChanged);
            mTools.Items.Add(mBlockGrid);

            mPixelGrid = new ToolStripButton();
            mPixelGrid.CheckOnClick = true;
            mPixelGrid.Text = "Pixels";
            mPixelGrid.AutoSize = true;
            mPixelGrid.CheckedChanged += new EventHandler(GridButtonsChanged);
            mTools.Items.Add(mPixelGrid);

            mFFCombo = new ToolStripComboBox();            
            mFFCombo.Items.AddRange(new string[] { "Frame", "TopField", "BottomField" });
            mFFCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mFFCombo.SelectedIndex = 0;
            mFFCombo.SelectedIndexChanged += new EventHandler(FFModeChanged);
            mTools.Items.Add(mFFCombo);


            mGridColor = new ToolStripButton();
            mGridColor.Text = "Color";
            mGridColor.AutoSize = true;
            mGridColor.Click += new EventHandler(GridColorClicked);
            mTools.Items.Add(mGridColor);

            mGridColorLabel = new ToolStripLabel();
            mGridColorLabel.Text = " ";
            mGridColorLabel.Click += new EventHandler(GridColorClicked);
            mTools.Items.Add(mGridColorLabel);

            ToolStripButton flip = new ToolStripButton();
            flip.Text = "[Flip]";
            flip.AutoSize = true;
            flip.Click += new EventHandler(GridColorFlip);
            mTools.Items.Add(flip);

            SplitContainer topSplit = new SplitContainer();

            topSplit.Orientation = Orientation.Vertical;
            topSplit.Dock = DockStyle.Fill;
            topSplit.BorderStyle = BorderStyle.FixedSingle;

            FlowLayoutPanel paramPanel = new FlowLayoutPanel();
            paramPanel.FlowDirection = FlowDirection.TopDown;
            paramPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            paramPanel.AutoSize = true;
            paramPanel.WrapContents = false;
            paramPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
            mFiles = new FileInterfacePanel[Constants.kNumberOfChannels];
            for (int i = 0; i < Constants.kNumberOfChannels; ++i)
            {
                mFiles[i] = new FileInterfacePanel(mAppState, i);
                paramPanel.Controls.Add(mFiles[i]);
            }

            paramPanel.Size = paramPanel.PreferredSize;
            topSplit.Panel1.Controls.Add(paramPanel);
            topSplit.Panel1.AutoScroll = true;
            topSplit.Panel1.Anchor = AnchorStyles.Left;

            mDisplay.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            topSplit.Panel2.Controls.Add(mDisplay);
            topSplit.Panel2.AutoScroll = true;
            if (_bgimageStream != null) 
            {
                topSplit.Panel2.BackgroundImageLayout = ImageLayout.Tile ;
                topSplit.Panel2.BackgroundImage = new Bitmap(_bgimageStream);
            }

//             mDisplay.BackColor = System.Drawing.Color.Blue;
            
            StatusStrip a_strip = new StatusStrip();
            mStatus = new ToolStripStatusLabel();
            a_strip.Items.Add(mStatus);

            mStatus.Text = "XXXXX";
            Controls.Add(topSplit);
            this.Controls.Add(mTools);
            Controls.Add(mMenu);
            this.Controls.Add(a_strip);

            // topFlow.Controls.Add(topSplit);
            // this.Controls.Add(topFlow);

            Width = 600;
            Height = 1000;
            
            mAppState.ToMeasure = 0;
            mTrackerCombo.SelectedIndex = 0;
            
            topSplit.SplitterDistance = paramPanel.Width;
            topSplit.FixedPanel = FixedPanel.Panel1;

            mAppState.ReplaceNotifier(this);
            SetGridColor(Color.FromArgb(255, Color.Black));
            SetStatus("Idle", false);

            Text = "YUV3";
            for (int i =0 ;i < filesToLoad.Length && i < mFiles.Length; ++i)
            {
                mFiles[i].AttemptToLoad(filesToLoad[i]);
            }
        }

        public void SetGridColor(Color c)
        {
            mGridColorLabel.BackColor = c;
            Display.SetGridColor(c);
        }

        public void GridColorFlip(Object sender, EventArgs e)
        {
            Color x = mGridColorLabel.BackColor;
            SetGridColor(Color.FromArgb(255, 255-x.R, 255-x.G, 255-x.B));
        }

        public void GridColorClicked(Object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = false;
            cd.Color = mGridColorLabel.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                SetGridColor(cd.Color);
            }
        }
        

        public void SetOperation(MathsOperation op)
        {
            Display.MathsOperation = op;
        }

        public MathsOperation GetOperation()
        {
            return Display.MathsOperation;
        }

        public void OnSubtractChanged(Object sender, EventArgs e)
        {
            if (mSubtractButton.Checked)
            {
                /* Let's see what we can do. */
                SetOperation(MathsOperation.SubtractAB);
            }
            else
            {
                SetOperation(MathsOperation.None);
            }
            Display.ImagesChanged();
        }

        public void SetVisible(int which, bool is_visible)
        {
            mFiles[which].SetVisible(is_visible);
        }

        public void OnZoomChanged(Object sender, EventArgs e)
        {
            mZoomBox.ForeColor = System.Drawing.Color.Blue;
        }

        public void OnZoomKeyDown(Object sender, KeyEventArgs e)
        {
            double new_zoom;
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            if (double.TryParse(mZoomBox.Text, out new_zoom))
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
            mAppState.Zoom = mAppState.Zoom + 0.2;
            mZoomBox.ForeColor = System.Drawing.Color.Black;
            mZoomBox.Text = mAppState.Zoom.ToString();
        }

        public void FFModeChanged(Object sender, EventArgs e)
        {
            FrameFieldMode ff = FrameFieldMode.FrameField;
            switch (mFFCombo.SelectedIndex)
            {
            case 0: ff = FrameFieldMode.FrameField; break;
            case 1: ff = FrameFieldMode.MBAFF; break;
            default: break;
            }
            Display.SetFFMode(ff);
        }

        public void GridButtonsChanged(Object sender, EventArgs e)
        {
            Display.SetGridMode(
                mMBGrid.Checked,
                mBlockGrid.Checked,
                mPixelGrid.Checked);
        }


        public void TrackerIndexChanged(Object sender, EventArgs e)
        {
            mAppState.ToMeasure = mTrackerCombo.SelectedIndex;
        }

        public void OnSoftScaleChanged(Object sender, EventArgs e)
        {
            mAppState.SoftScaling = mSoftScaling.Checked;
        }

        public void OnDecreaseZoom(Object sender, EventArgs e)
        {
            if (mAppState.Zoom > 0)
            {
                mAppState.Zoom = mAppState.Zoom - 0.2;
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

        public void SetFrameData(int which, int frame, bool has_fid, uint frame_id, ulong offset, uint sum)
        {
            if (mFiles[which] != null)
            {
                mFiles[which].SetFrameData(frame, has_fid, frame_id, offset, sum);
            }
        }

        public void SetIsMaths(bool is_maths)
        {
            if (is_maths)
            {
                mTrackerCombo.SelectedText = "(maths mode)";
                mTrackerCombo.Enabled = false;
            }
            else
            {
                mTrackerCombo.SelectedIndex = mTrackerCombo.SelectedIndex;
                mTrackerCombo.Enabled = true;
            }
        }


    }
}

/* End file */
