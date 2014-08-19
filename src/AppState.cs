/* AppState.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;

namespace yuv3
{
    public class AppState
    {
        YUVFileAccess mYUVFile;
        MainWindow mW;

        public void SetStatus(string in_status, bool withDialog = false)
        {            
            mW.SetStatus(in_status, withDialog);
        }


        public void SetMainWindow(MainWindow in_mw) 
        {
            mW = in_mw;
        }

        public void LoadFile(string in_name)
        {
            mYUVFile.LoadFile(in_name);
            SetStatus("Loaded " + in_name);
        }

        public AppState()
        {
            mW = null;
            mYUVFile = new YUVFileAccess(null, 640, 480, YUVFileFormat.YUYV);
        }
    }
}