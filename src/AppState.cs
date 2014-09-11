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
        public int mZoom;

        public int Zoom
        {
            get
            {
                return mZoom;
            }
            
            set
            {
                mZoom = value;
                mW.Display.ImagesChanged();
            }
        }

        // Stores which index is in which register: -1 => none.
        public int[] mRegisters;

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
                mW.Display.UpdateLayer(which, mFiles[which]);
            }
        }

        public void SetVisible(int layer, bool visible)
        {
            mW.Display.UpdateLayer(layer, (visible ? mFiles[layer] : null));
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
            mW.SetVisible(which, true);
            /* Now reconstruct the display */
            mW.Display.UpdateLayer(which, mFiles[which]);
        }

        public void ReplaceNotifier(IStatusNotifier isn)
        {
            mNotifier = isn;
            for (int i = 0; i < mFiles.Length; ++i)
            {
                mFiles[i].ReplaceNotifier(isn);
            }
        }

        public void StoreToRegister(int regno, int which)
        {
            if (mRegisters[regno] != -1)
            {
                mW.ClearRegister(regno, mRegisters[regno]);
            }
            mRegisters[regno] = which;
        }

        public AppState()
        {
            mW = null;
            mFiles = new YUVFile[Constants.kNumberOfChannels];
            mRegisters = new int[Constants.kRegisters];
            mZoom = 1;
            for (int i =0 ;i < mRegisters.Length; ++i)
            {
                mRegisters[i] = -1;
            }
            mNotifier = new ConsoleStatusNotifier();
            for (int i = 0; i < mFiles.Length; ++i)
            {
                mFiles[i] = new YUVFile(mNotifier);
            }
        }
    }
}