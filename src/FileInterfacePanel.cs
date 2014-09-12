/* FileInterfacePanel.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;

namespace yuv3
{
    public class FileInterfacePanel : FlowLayoutPanel
    {
        int mWhich;
        TextBox mDimBox;
        TextBox mFrameBox;
        TextBox mIDBox;
        TextBox mOffsetBox, mSumBox;
        Button mFileButton;
        AppState mAppState;
        ComboBox mFormat;
        int mW, mH;
        int mFrame;
        YUVFileFormat mF;
        CheckBox mVisibleCheckBox;
        CheckBox[] mRegisterChecks;
        double mPixelsPerPoint;

        public int FileWidth
        {
            get
            {
                return mW;
            }
        }

        public int FileHeight
        {
            get
            {
                return mH;
            }
        }
        
        public YUVFileFormat Format
        {
            get
            {
                return mF;
            }
        }

        public int Frame
        {
            get
            {
                return mFrame;
            }
        }

        public FileInterfacePanel(AppState inAppState, int which) 
        {
            this.FlowDirection = FlowDirection.TopDown;
            // this.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            // this.Dock = DockStyle.Top;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.mWhich = which;
            this.mFrame = 0;
            this.mAppState = inAppState;
            this.WrapContents = false;
            this.BorderStyle = BorderStyle.Fixed3D;
            
            TableLayoutPanel title = new TableLayoutPanel();
            title.ColumnCount = 2;
            title.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            title.AutoSize = true;
            title.AutoSizeMode = AutoSizeMode.GrowOnly;
            title.BackColor = System.Drawing.Color.DarkGray;
            title.ForeColor = System.Drawing.Color.White;

            Label a_title = new Label();
            a_title.Text = string.Format("File#{0}", 
                                         this.mWhich);
            a_title.AutoSize = true;
            a_title.TextAlign = ContentAlignment.MiddleCenter;
            a_title.Font = new Font("Times New Roman", 14.0f);
            a_title.Padding = new Padding(0, 4, 0, 0);
            title.Controls.Add(a_title);


            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.FlowDirection = FlowDirection.LeftToRight;
            buttons.AutoSize = true;
            buttons.AutoSizeMode = AutoSizeMode.GrowOnly;

            {
                Graphics g = this.CreateGraphics();
                mPixelsPerPoint = (double)g.DpiX / 72.0;
                g.Dispose();
            }
            
            mVisibleCheckBox = new CheckBox();
            mVisibleCheckBox.Text = "On";
            mVisibleCheckBox.Appearance = Appearance.Button;
            mVisibleCheckBox.CheckedChanged += new EventHandler(OnVisibleChanged);
            mVisibleCheckBox.Width = (int)(mPixelsPerPoint * mVisibleCheckBox.Font.SizeInPoints * 3);
            buttons.Controls.Add(mVisibleCheckBox);
                

            /* Now, we have a series of checkbox buttons that say which register
             * we are in.
             */
            mRegisterChecks = new CheckBox[Constants.kRegisters];
            for (int i =0 ;i < Constants.kRegisters; ++i)
            {
                // Oh, C#, you _do_ have an interesting closure implementation and
                // the usual \x.\y.(x) notation doesn't work for you.
                int regno = i;
                CheckBox cb = new CheckBox();
                cb.Text = String.Format("{0}", (char)('A' + i));
                cb.Width = (int)(mPixelsPerPoint * cb.Font.SizeInPoints * 2);
                cb.Appearance = Appearance.Button;
                cb.CheckedChanged += new EventHandler
                    ( (Object sender, EventArgs e) => {  StoreToRegister(sender, e, regno); } );
                buttons.Controls.Add(cb);
                mRegisterChecks[i] = cb;
            }

            title.Controls.Add(buttons);
            
            TableLayoutPanel inner = new TableLayoutPanel();
            inner.ColumnCount = 2;
            inner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inner.AutoSize = true;
            inner.AutoSizeMode = AutoSizeMode.GrowOnly;
            inner.BackColor = System.Drawing.Color.LightGray;

            Label f = new Label();
            f.Text = "File: ";
            f.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(f);
            mFileButton = new Button();
            mFileButton.Text = "None";
            mFileButton.AutoSize = true;
            mFileButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            mFileButton.Click += new EventHandler(OnFileOpen);
            inner.Controls.Add(mFileButton);

            Label w = new Label(); 
            w.Text = "Dimensions: ";
            w.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(w);
            mDimBox = new TextBox();
            mDimBox.Text = string.Format("{0} x {1}", 
                                           Constants.kDefaultWidth,
                                           Constants.kDefaultHeight);
            mW = Constants.kDefaultWidth;
            mH = Constants.kDefaultHeight;
            mF = Constants.kDefaultFormat;
            inner.Controls.Add(mDimBox);
            mDimBox.TextChanged += new EventHandler(OnDimChanged);
            mDimBox.KeyDown += new KeyEventHandler(OnDimKeyDown);

            f = new Label();
            f.Text = "Format: ";
            f.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(f);

            mFormat = new ComboBox();
            string[] formats = new string[]{ "YUV420I", "YUV420P", "YUYV", "YVYU", "Y8", "Y16", "None" };
            mFormat.Items.AddRange(formats);
            mFormat.SelectedValueChanged += new System.EventHandler(OnFormatChanged);
            mFormat.SelectedIndex = 0;
            mFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            inner.Controls.Add(mFormat);

            f = new Label();
            f.Text = "Frame: ";
            f.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(f);

            FlowLayoutPanel frameSpinner = new FlowLayoutPanel();
            frameSpinner.AutoSize = true;
            frameSpinner.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Button frame_up = new Button(); 
            frame_up.Text = "+";
            frame_up.Click += new EventHandler(OnFrameUp);
            frame_up.AutoSize = true;
            Button frame_down = new Button();
            frame_down.Text = "-";
            frame_down.AutoSize = true;
            frame_down.Click += new EventHandler(OnFrameDown);
            mFrameBox = new TextBox();
            mFrameBox.Text = "0";
            mFrameBox.Width = (int)(mPixelsPerPoint * mFrameBox.Font.SizeInPoints * 4);
            mFrameBox.TextChanged += new EventHandler(OnFrameChanged);
            mFrameBox.KeyDown += new KeyEventHandler(OnFrameKeyDown);
            
            frameSpinner.Controls.Add(frame_down);           
            frameSpinner.Controls.Add(mFrameBox);
            frameSpinner.Controls.Add(frame_up);           

            inner.Controls.Add(frameSpinner);


            f = new Label();
            f.Text = "ID: ";
            f.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(f);

            FlowLayoutPanel idFlow = new FlowLayoutPanel();
            idFlow.AutoSize = true;
            idFlow.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            mIDBox = new TextBox();
            mIDBox.Width = (int)(mPixelsPerPoint * mFrameBox.Font.SizeInPoints * 8);
            mIDBox.TextChanged += new EventHandler(OnIDChanged);
            mIDBox.KeyDown += new KeyEventHandler(OnIDKeyDown);
            idFlow.Controls.Add(mIDBox);
            
            f = new Label();
            f.Text = "@";
            f.Padding = new Padding(0, 4, 0, 0);
            f.AutoSize = true;
            idFlow.Controls.Add(f);

            mOffsetBox = new TextBox();
            mOffsetBox.Width = (int)(mPixelsPerPoint * mFrameBox.Font.SizeInPoints * 8);
            mOffsetBox.ReadOnly = true;
            idFlow.Controls.Add(mOffsetBox);

            inner.Controls.Add(idFlow);
            

            f = new Label();
            f.Text = "Sum: ";
            f.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(f);

            mSumBox = new TextBox();
            mSumBox.Width = (int)(mPixelsPerPoint * mFrameBox.Font.SizeInPoints * 8);
            mSumBox.ReadOnly = true;
            inner.Controls.Add(mSumBox);


            this.Controls.Add(title);
            this.Controls.Add(inner);
        }


        public void SetNewFormat(YUVFileFormat ff)
        {
            switch (ff)
            {
            case YUVFileFormat.YUV420I: mFormat.SelectedIndex = 0;
                break;
            case YUVFileFormat.YUV420P: mFormat.SelectedIndex = 1;
                break;
            case YUVFileFormat.YUYV: mFormat.SelectedIndex = 2;
                break;
            case YUVFileFormat.YVYU: mFormat.SelectedIndex = 3;
                break;
            case YUVFileFormat.Y8: mFormat.SelectedIndex = 4;
                break;
            case YUVFileFormat.Y16:  mFormat.SelectedIndex = 5;
                break;
            default:
                mFormat.SelectedIndex = 4;
                break;
            }
        }

        public void StoreToRegister(Object sender, EventArgs e, int regno)
        {
            mAppState.StoreToRegister(regno, ((CheckBox)sender).Checked ? mWhich : -1);
        }
        
        public void SetRegister(int reg, bool is_set)
        {
            mRegisterChecks[reg].Checked = is_set;
        }

        public void SetNewDimensions(int w, int h)
        {
            mW = w; mH = h;
            mDimBox.Text = string.Format("{0} x {1}", 
                                           w, h);
            mDimBox.ForeColor = System.Drawing.Color.Black;
        }

        
        public void SetVisible(bool is_visible)
        {
            mVisibleCheckBox.CheckState = (is_visible ? CheckState.Checked : CheckState.Unchecked);
        }

        public void OnVisibleChanged(Object sender, EventArgs e)
        {
            mAppState.SetVisible(mWhich,
                                 (mVisibleCheckBox.CheckState == CheckState.Checked));
        }


        public void OnFormatChanged(Object sender, EventArgs e)
        {
            YUVFileFormat result = YUVFileFormat.Unknown;

            switch (mFormat.SelectedIndex)
            {
            case 0: 
                result = YUVFileFormat.YUV420I;
                break;
            case 1:
                result = YUVFileFormat.YUV420P;
                break;
            case 2:
                result = YUVFileFormat.YUYV;
                break;
            case 3:
                result = YUVFileFormat.YVYU;
                break;
            case 4:
                result = YUVFileFormat.Y8;
                break;
            case 5: 
                result = YUVFileFormat.Y16;
                break;
            default:
                // Hmm .. 
                break;
            }
            if (result != mF)
            {
                mF = result;
                mAppState.UserSet(mWhich, mW, mH, mFrame, result);
            }

        }

        void OnFrameUp(Object sender, EventArgs e)
        {
            ++mFrame;
            mFrameBox.Text = mFrame.ToString();
            mAppState.UserSet(mWhich, mW, mH, mFrame, mF);            
        }

        void OnFrameDown(Object sender, EventArgs e)
        {
            if (mFrame > 0)
            {
                --mFrame;
                mFrameBox.Text = mFrame.ToString();
                mAppState.UserSet(mWhich, mW, mH, mFrame, mF);            
            }
        }

        void OnIDChanged(Object sender, EventArgs e)
        {
            mIDBox.ForeColor = System.Drawing.Color.Blue;
        }

        void OnFrameChanged(Object sender, EventArgs e)
        {
            mFrameBox.ForeColor = System.Drawing.Color.Blue;
        }

        void OnDimChanged(Object sender, EventArgs e)
        {
            // Indicate a change.
            mDimBox.ForeColor = System.Drawing.Color.Blue;
        }

        void OnIDKeyDown(Object sender, KeyEventArgs e)
        {
            uint new_id;

            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            if (Utils.TryParseNumber(mIDBox.Text, out new_id))
            {
                uint frame_number;
                bool ok;
                ok = mAppState.SeekID(mWhich, mFrame, new_id, out frame_number);
                if (ok)
                {
                    mIDBox.ForeColor = System.Drawing.Color.Black;
                    /* And go there !*/
                    mFrame = (int)frame_number;
                    mAppState.UserSet(mWhich, mW, mH, mFrame, mF);            
                }
                else
                {
                    mIDBox.ForeColor = System.Drawing.Color.Gray;
                }
            }
            else 
            {
                mIDBox.ForeColor = System.Drawing.Color.Red;
            }
        }

        void OnFrameKeyDown(Object sender, KeyEventArgs e)
        {
            uint new_frame;
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            
            if (Utils.TryParseNumber(mFrameBox.Text, out new_frame))
            {
                mFrame = (int)new_frame;
                mAppState.UserSet(mWhich, mW, mH, mFrame, mF);
                mFrameBox.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                mFrameBox.ForeColor = System.Drawing.Color.Red;
            }
        }

        void OnDimKeyDown(Object sender, KeyEventArgs e)
        {
            int new_width;
            int new_height;

            if (e.KeyCode != Keys.Enter)
            {
                // No-one cares.
                return;
            }

            /* Find the 'x' */
            string the_text = mDimBox.Text;
            int x = the_text.IndexOf("x");
            if (x >= 0)
            {
                string left = the_text.Substring(0, x-1);
                string right = the_text.Substring(x+1);
                left = Regex.Replace(left, @"\s", "");
                right = Regex.Replace(right, @"\s", "");
                if (int.TryParse(left, out new_width) && 
                    int.TryParse(right, out new_height))
                {
                    mW = new_width;
                    mH = new_height;
                    mDimBox.ForeColor = System.Drawing.Color.Black;
                    mAppState.UserSet(mWhich, new_width, new_height, 
                                      mFrame, mF);
                }
                else
                {
                    mDimBox.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        YUVFileFormat FormatFromExtension(string ext)
        {
            if (ext == "yuv420i")
            {
                return YUVFileFormat.YUV420I;
            }
            else if (ext == "yuv420p")
            {
                return YUVFileFormat.YUV420P;
            }
            else if (ext == "yuyv" || ext == "yuyv422")
            {
                return YUVFileFormat.YUYV;
            }
            else if (ext == "yvyu" || ext == "yvyu422")
            {
                return YUVFileFormat.YVYU;
            }
            else if (ext == "y8")
            {
                return YUVFileFormat.Y8;
            }
            else if (ext == "y16")
            {
                return YUVFileFormat.Y16;
            }
            return YUVFileFormat.Unknown;
        }

        void OnFileOpen(object sender, EventArgs e)
        {
            OpenFileDialog ofDlg = new OpenFileDialog();
            DialogResult res = ofDlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                AttemptToLoad(ofDlg.FileName);
            }
        }

        public void AttemptToLoad(String fn)
        {
            try
            {
                int w, h;
                YUVFileFormat f;
                
                /* See if the file names some parameters */
                Regex dimensions = new Regex("_([0-9]+)x([0-9]+)");
                Match m = dimensions.Match(fn);
                if (m.Value != String.Empty)
                {
                    w = int.Parse(m.Groups[1].Value);
                    h = int.Parse(m.Groups[2].Value);
                }
                else
                {
                    w = FileWidth;
                    h = FileHeight;
                }
                
                /* Find the extension */
                int last_dot = fn.LastIndexOf('.');
                if (last_dot > 0)
                {
                    string extn = fn.Substring(last_dot+1);
                    f = FormatFromExtension(extn);
                    if (f == YUVFileFormat.Unknown)
                    {
                        f = Format;
                    }
                }
                else
                {
                    f = Format;
                }
                
                // Console.WriteLine(String.Format("{0} x{1}", w,h));
                mAppState.LoadFile(mWhich, fn,
                                   w, h, Frame, f);
                mFileButton.Text = fn;
                
                //Console.WriteLine(String.Format(" Back .. "));
                // Now that we are sure the new load is OK .. 
                bool f_changed = (f != Format);
                bool s_changed = (w != FileWidth || h != FileHeight);
                mF = f; mW = w; mH = h;
                if (f_changed)
                {
                    SetNewFormat(f);
                }
                if (s_changed)
                {
                    SetNewDimensions(w,h);
                }
            }
            catch (Exception x)
            {
                mAppState.SetStatus("Cannot open " + fn + " -" + x, true);
                mFileButton.Text = "(none)";
            }
        }

        public void SetFrameData(int frame, bool has_fid, uint frame_id, ulong offset, uint checksum)
        {
            if (has_fid)
            {
                mIDBox.Text = "0x" + frame_id.ToString("x8");
                mIDBox.ForeColor = System.Drawing.Color.Purple;
            }
            else
            {
                mIDBox.Text = "(none)";
                mIDBox.ForeColor = System.Drawing.Color.Gray;
            }
            mOffsetBox.Text = offset.ToString();
            mSumBox.Text = "0x" + checksum.ToString("x8");


            mFrameBox.Text = frame.ToString();
            mFrameBox.ForeColor = System.Drawing.Color.Black;
        }

    }
    
}