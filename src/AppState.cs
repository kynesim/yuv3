/* AppState.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;

namespace yuv3
{
    public class AppState
    {
        public YUVFile[] mFiles;
        public MainWindow mW;

        public void SetStatus(string in_status, bool withDialog = false)
        {            
            mW.SetStatus(in_status, withDialog);
        }

        public void UserSet(int which, int w, int h)
        {
            // mFiles[which].Set(w,h);
        }

        public void SetMainWindow(MainWindow in_mw) 
        {
            mW = in_mw;
        }

        public void LoadFile(int which, string in_name, int w, int h)
        {
            // mYUVFile.LoadFile(in_name);
            //m SetStatus("Loaded " + in_name);
        }

        public AppState()
        {
            mW = null;
            mFiles = new YUVFile[Constants.kNumberOfChannels];
            for (int i = 0; i < mFiles.Length; ++i)
            {
                mFiles[i] = new YUVFile();
            }
        }
    }
}