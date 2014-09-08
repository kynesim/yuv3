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
        Button mFileButton;
        AppState mAppState;
        ComboBox mFormat;
        int mW, mH;
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


        public FileInterfacePanel(AppState inAppState, int which) 
        {
            this.ColumnCount = 1;
            this.Anchor = AnchorStyles.Left | AnchorStyles.Right |
                AnchorStyles.Top | AnchorStyles.Bottom;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Red;
            this.mWhich = which;
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
            mFormat.Text = "YUV420";
            string[] formats = new string[]{ "YUV420", "YUYV", "YVYU", "None" };
            mFormat.Items.AddRange(formats);
            mFormat.SelectedValueChanged += new System.EventHandler(OnFormatChanged);
            inner.Controls.Add(mFormat);

            this.Controls.Add(inner);
        }


        public void SetNewFormat(YUVFileFormat ff)
        {
            switch (ff)
            {
            case YUVFileFormat.YUV420: mFormat.SelectedIndex = 0;
                break;
            case YUVFileFormat.YUYV: mFormat.SelectedIndex = 1;
                break;
            case YUVFileFormat.YVYU: mFormat.SelectedIndex = 2;
                break;
            default:
                mFormat.SelectedIndex = 3;
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
                result = YUVFileFormat.YUV420;
                break;
            case 1:
                result = YUVFileFormat.YUYV;
                break;
            case 2:
                result = YUVFileFormat.YVYU;
                break;
            default:
                // Hmm .. 
                break;
            }
            mF = result;
            mAppState.UserSet(mWhich, mW, mH, result);
        }


        void OnDimChanged(Object sender, EventArgs e)
        {
            // Indicate a change.
            mDimBox.ForeColor = System.Drawing.Color.Blue;
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
                    mAppState.UserSet(mWhich, new_width, new_height, mF);
                }
                else
                {
                    mDimBox.ForeColor = System.Drawing.Color.Gray;
                }
            }
        }

        void OnFileOpen(object sender, EventArgs e)
        {
            OpenFileDialog ofDlg = new OpenFileDialog();
            DialogResult res = ofDlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                try
                {
                    mAppState.LoadFile(mWhich, ofDlg.FileName, FileWidth, FileHeight, Format);
                    mFileButton.Text = ofDlg.FileName;
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