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
        public double mZoom;
        // Stores which index is in which register: -1 => none.
        public int[] mRegisters;
        public int mToMeasure;
        public bool mSoftScaling;

        public bool SoftScaling
        {
            get
            {
                return mSoftScaling;
            }
            set
            {
                mSoftScaling =value;
                mW.Display.ImagesChanged();
            }
        }

        public int ToMeasure
        {
            get
            {
                return mToMeasure;
            }

            set
            {
                mToMeasure = value;
            }

        }

        public double Zoom
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


        public void QueryYUV(int idx, int x, int y, out int cy, out int cu, out int cv)
        {
            YUVFile f = mFiles[idx];
            if (f != null)
            {
                f.QueryYUV(x,y,out cy, out cu, out cv);
            }
            else
            {
                cy = cu = cv = -1;
            }
        }

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
                UpdateInterface(which);
            }
        }
        
        
        public bool SeekID(int which, int from_frame, uint id, out uint frame_number)
        {
            if (mFiles[which] != null)
            {
                uint cur_fid;
                bool ok;
                ok = mFiles[which].GetFrameID(out cur_fid);
                if (ok && cur_fid == id)
                {
                    ++from_frame;
                }
                return mFiles[which].SeekID(id, from_frame, out frame_number);
            }
            else
            {
                frame_number = 0xFFFFFFFF;
                return false;
            }
        }

        public void SetVisible(int layer, bool visible)
        {
            mW.Display.UpdateLayer(layer, (visible ? mFiles[layer] : null));
            UpdateInterface(layer);
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
            UpdateInterface(which);
        }
        
        public void UpdateInterface(int which)
        {
            uint new_fid;
            bool has_fid;

            if (mW != null && mFiles[which] != null)
            {
                has_fid = mFiles[which].GetFrameID(out new_fid);
                mW.SetFrameData(which, mFiles[which].Frame,
                                has_fid, new_fid, (ulong)mFiles[which].FileOffsetOfPicture, 
                                mFiles[which].Checksum);
            }
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
            mW.Display.UpdateMaths();
        }

        public AppState()
        {
            mW = null;
            mToMeasure = 0;
            mFiles = new YUVFile[Constants.kNumberOfChannels];
            mRegisters = new int[Constants.kRegisters];
            mZoom = 1.0;
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

        public void SetOperation(MathsOperation op)
        {
            mW.SetOperation(op);
        }

        public MathsOperation GetOperation()
        {
            return mW.GetOperation();
        }

        public YUVFile FileForRegister(int reg)
        {
            if (mRegisters[reg] >= 0) 
            {
                return mFiles[mRegisters[reg]];
            }
            else
            {
                return null;
            }
        }

        public void SetIsMaths(bool is_maths)
        {
            mW.SetIsMaths(is_maths);
        }
        
    }


}