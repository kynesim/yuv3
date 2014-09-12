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


        public MainWindow(AppState inAppState)
        {
            // Faff with reflection.
            try
            {
                _assembly = Assembly.GetExecutingAssembly();
                _bgimageStream = _assembly.GetManifestResourceStream("MainBackground.png");
                Console.WriteLine("is = {0}", _bgimageStream == null);
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
            SetStatus("Idle", false);

            Text = "YUV3";
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

        public void TrackerIndexChanged(Object sender, EventArgs e)
        {
            mAppState.ToMeasure = mTrackerCombo.SelectedIndex;
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


    }
}

/* End file */
