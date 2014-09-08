/* FileInterfacePanel.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;

namespace yuv3
{
    public class FileInterfacePanel : TableLayoutPanel
    {
        int mWhich;
        TextBox mWidthBox, mHeightBox;
        Button mFileButton;
        AppState mAppState;

        public int FileWidth
        {
            get
            {
                return int.Parse(mWidthBox.Text);
            }
        }

        public int FileHeight
        {
            get
            {
                return int.Parse(mHeightBox.Text);
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
            w.Text = "Width: ";
            w.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(w);
            mWidthBox = new TextBox();
            inner.Controls.Add(mWidthBox);
            mWidthBox.Anchor = AnchorStyles.Right | AnchorStyles.Left;

            Label h = new Label(); 
            h.Text = "Height: ";
            h.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            inner.Controls.Add(h);
            mHeightBox = new TextBox();
            inner.Controls.Add(mHeightBox);
            mHeightBox.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this.Controls.Add(inner);
             

        }


        public void SetNewDimensions(int w, int h)
        {
            mWidthBox.Text = w.ToString();
            mHeightBox.Text = h.ToString();
        }

        void OnWidthChanged(Object sender, EventArgs e)
        {
            int new_width;
            if (int.TryParse(mWidthBox.Text, out new_width))
            {
                mAppState.UserSetWidth(mWhich, new_width);
            }
        }

        void OnHeightChanged(Object sender, EventArgs e)
        {
            int new_height;
            if (int.TryParse(mHeightBox.Text, out new_height))
            {
                mAppState.UserSetHeight(mWhich, new_height);
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
                    //mAppState.LoadFile(ofDlg.FileName, FileWidth, FileHeight);
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