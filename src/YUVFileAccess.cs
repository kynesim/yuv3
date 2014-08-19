/* YUVFileAccess.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace yuv3
{
    enum YUVFileFormat
    {
        YUV420,
        YUYV,
        YVYU
    }

    class YUVFileAccess
    {
        string mFileName;
        int mWidth;
        int mHeight;
        YUVFileFormat mFormat;
        BinaryReader mReader;
        FileStream mStream;

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
            }
            else
            {
                mStream = null;
                mReader = null;
            }
            mFileName = in_file;
        }
        
        public YUVFileAccess(string in_file, int w, int h, 
                      YUVFileFormat fmt)
        {
            mWidth = w;
            mHeight = h;
            mFormat = fmt;
            LoadFile(in_file);
        }
    }
}

/* End file */
