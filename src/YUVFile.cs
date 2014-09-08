/* YUVFileAccess.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace yuv3
{
    public enum YUVFileFormat
    {
        YUV420,
        YUYV,
        YVYU,
        Unknown
    }

    public class YUVFile
    {
        public string mFileName;
        public int mWidth;
        public int mHeight;
        public YUVFileFormat mFormat;
        BinaryReader mReader;
        FileStream mStream;
        bool mLoaded;

        public bool Loaded
        {
            get 
            {
                return mLoaded;
            }
        }


        public void CloseFile()
        {
            mLoaded = false;
        }

        public void LoadFile(string in_file)
        {
            if (in_file != null)
            {
                FileStream result_stream = 
                    new FileStream(in_file, FileMode.Open);
                BinaryReader new_reader = 
                    new BinaryReader(result_stream);

                mStream = result_stream;
                mReader = new_reader;
                mLoaded = true;
            }
            else
            {
                mStream = null;
                mReader = null;
                mLoaded = false;
            }
            mFileName = in_file;
        }

        public void Set(int w, int h, YUVFileFormat fmt)
        {
            mWidth = w;
            mHeight = h;
            mFormat = fmt;
        }
        
        public YUVFile()
        {
            mLoaded = false;
        }
    }
}

/* End file */
