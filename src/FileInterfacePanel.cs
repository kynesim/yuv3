/* FileInterfacePanel.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace yuv3
{
    public class FileInterfacePanel : TableLayoutPanel
    {
        int mWhich;
        TextBox mDimBox;
        TextBox mFrameBox;
        Button mFileButton;
        AppState mAppState;
        ComboBox mFormat;
        int mW, mH, mFrame;
        YUVFileFormat mF;

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
            this.ColumnCount = 1;
            this.Anchor = AnchorStyles.Left | AnchorStyles.Right |
                AnchorStyles.Top | AnchorStyles.Bottom;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Red;
            this.mWhich = which;
            this.mFrame = 0;
            this.mAppState = inAppState;

            TableLayoutPanel title = new TableLayoutPanel();
            title.AutoSize = true;
            title.ColumnCount = 2;

            Label a_title = new Label();
            a_title.Text = string.Format("File#{0}", 
                                         this.mWhich);
            a_title.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            title.Controls.Add(a_title);


            TableLayoutPanel buttons = new TableLayoutPanel();

            buttons.AutoSize = true;
            buttons.BackColor = System.Drawing.Color.Yellow;
            buttons.RowCount = 1;
            buttons.ColumnCount = 4;
            buttons.Anchor = AnchorStyles.Right;
            
            CheckBox on = new CheckBox();
            on.Text = "On";
            on.Appearance = Appearance.Button;
            buttons.Controls.Add(on);

            Button up = new Button();
            up.Text = "Up";
            buttons.Controls.Add(up);

            Button down = new Button();
            down.Text = "Down";
            buttons.Controls.Add(down);


            title.Controls.Add(buttons);
            
            

            this.Controls.Add(title);
            
            TableLayoutPanel inner = new TableLayoutPanel();
            inner.ColumnCount = 2;
            inner.RowCount = 4;
            inner.Anchor = AnchorStyles.Left | AnchorStyles.Right |
               AnchorStyles.Top;
            inner.AutoSize = true;

            Label f = new Label();
            f.Text = "File: ";
            f.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(f);
            mFileButton = new Button();
            mFileButton.Text = "None";
            mFileButton.Click += new EventHandler(OnFileOpen);
            mFileButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
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
            mDimBox.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            mDimBox.TextChanged += new EventHandler(OnDimChanged);
            mDimBox.KeyDown += new KeyEventHandler(OnDimKeyDown);

            f = new Label();
            f.Text = "Format: ";
            f.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(f);

            mFormat = new ComboBox();
            string[] formats = new string[]{ "YUV420I", "YUV420P", "YUYV", "YVYU", "None" };
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
            Button frame_up = new Button(); 
            frame_up.Text = "+";
            frame_up.Click += new EventHandler(OnFrameUp);
            frame_up.AutoSize = true;
            Button frame_down = new Button();
            frame_down.Text = "-";
            frame_down.Click += new EventHandler(OnFrameDown);
            frame_down.AutoSize = true;
            mFrameBox = new TextBox();
            mFrameBox.Text = "0";
            mFrameBox.TextChanged += new EventHandler(OnFrameChanged);
            mFrameBox.KeyDown += new KeyEventHandler(OnFrameKeyDown);
                
            frameSpinner.Controls.Add(mFrameBox);
            frameSpinner.Controls.Add(frame_up);
            frameSpinner.Controls.Add(frame_down);

            inner.Controls.Add(frameSpinner);


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
            default:
                mFormat.SelectedIndex = 4;
                break;
            }
        }

        public void SetNewDimensions(int w, int h)
        {
            mW = w; mH = h;
            mDimBox.Text = string.Format("{0} x {1}", 
                                           w, h);
            mDimBox.ForeColor = System.Drawing.Color.Black;
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

        void OnFrameChanged(Object sender, EventArgs e)
        {
            mFrameBox.ForeColor = System.Drawing.Color.Blue;
        }

        void OnDimChanged(Object sender, EventArgs e)
        {
            // Indicate a change.
            mDimBox.ForeColor = System.Drawing.Color.Blue;
        }

        void OnFrameKeyDown(Object sender, KeyEventArgs e)
        {
            int new_frame;
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            
            if (int.TryParse(mFrameBox.Text, out new_frame))
            {
                mFrame = new_frame;
                mAppState.UserSet(mWhich, mW, mH, mFrame, mF);
                mFrameBox.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                mFrameBox.ForeColor = System.Drawing.Color.Gray;
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
                    mDimBox.ForeColor = System.Drawing.Color.Gray;
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
            else if (ext == "yuyv")
            {
                return YUVFileFormat.YUYV;
            }
            else if (ext == "yvyu")
            {
                return YUVFileFormat.YVYU;
            }
            return YUVFileFormat.Unknown;
        }

        void OnFileOpen(object sender, EventArgs e)
        {
            OpenFileDialog ofDlg = new OpenFileDialog();
            DialogResult res = ofDlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                try
                {
                    string fn = ofDlg.FileName;
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
                       
                    Console.WriteLine(String.Format("{0} x{1}", w,h));
                    mAppState.LoadFile(mWhich, ofDlg.FileName, 
                                       w, h, Frame, f);
                    mFileButton.Text = ofDlg.FileName;

                    // Now that we are sure the new load is OK .. 
                    mF = f; mW = w; mH = h;
                    if (f != Format)
                    {
                        SetNewFormat(f);
                    }
                    if (w != FileWidth || h != FileHeight)
                    {
                        SetNewDimensions(w,h);
                    }
                }
                catch (Exception x)
                {
                    mAppState.SetStatus("Cannot open " + ofDlg.FileName + " -" + x, true);
                    mFileButton.Text = "(none)";
                }
            }
        }



    }
    
}