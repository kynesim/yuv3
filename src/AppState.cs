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
        public IStatusNotifier mNotifier;

        public void SetStatus(string in_status, bool withDialog = false)
        {            
            mW.SetStatus(in_status, withDialog);
        }

        public void UserSet(int which, int w, int h, int frame, 
                            YUVFileFormat fmt)
        {
            if (mFiles[which] != null)
            {
                mFiles[which].Set(w,h, frame, fmt);
                mW.Display.UpdateLayer(which, mFiles[which], 128);
            }
        }

        public void SetMainWindow(MainWindow in_mw) 
        {
            mW = in_mw;
        }

        public void LoadFile(int which, string in_name, int w, int h, int frame,
                             YUVFileFormat fmt)
        {
            mFiles[which].CloseFile();
            mFiles[which].Set(w,h, frame, fmt);
            mFiles[which].LoadFile(in_name);
            mW.SetStatus("Loaded " + in_name, false);
            /* Now reconstruct the display */
            mW.Display.UpdateLayer(which, mFiles[which], 128);
        }

        public void ReplaceNotifier(IStatusNotifier isn)
        {
            mNotifier = isn;
            for (int i = 0; i < mFiles.Length; ++i)
            {
                mFiles[i].ReplaceNotifier(isn);
            }
        }

        public AppState()
        {
            mW = null;
            mFiles = new YUVFile[Constants.kNumberOfChannels];
            mNotifier = new ConsoleStatusNotifier();
            for (int i = 0; i < mFiles.Length; ++i)
            {
                mFiles[i] = new YUVFile(mNotifier);
            }
        }
    }
}