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
            this.RowCount = 2;
            this.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.mWhich = which;
            this.mAppState = inAppState;

            Label f = new Label();
            f.Text = "File: ";
            f.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(f, 0, 0);
            mFileButton = new Button();
            mFileButton.Text = "None";
            mFileButton.Click += new EventHandler(OnFileOpen);

            Label w = new Label(); 
            w.Text = "Width: ";
            w.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(w, 0, 0);
            mWidthBox = new TextBox();
            this.Controls.Add(mWidthBox, 1, 0);

            Label h = new Label(); 
            h.Text = "Height: ";
            h.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(w, 0, 1);
            mHeightBox = new TextBox();
            this.Controls.Add(mHeightBox, 1, 1);


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