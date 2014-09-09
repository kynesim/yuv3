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

        public void UserSet(int which, int w, int h, YUVFileFormat fmt)
        {
            if (mFiles[which] != null)
            {
                mFiles[which].Set(w,h, fmt);
                mW.Display.UpdateLayer(which, mFiles[which], 128);
            }
        }

        public void SetMainWindow(MainWindow in_mw) 
        {
            mW = in_mw;
        }

        public void LoadFile(int which, string in_name, int w, int h, YUVFileFormat fmt)
        {
            mFiles[which].CloseFile();
            mFiles[which].Set(w,h, fmt);
            mFiles[which].LoadFile(in_name);
            mW.SetStatus("Loaded " + in_name, false);
            /* Now reconstruct the display */
            mW.Display.UpdateLayer(which, mFiles[which], 128);
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