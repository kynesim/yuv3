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
        YUV420I,
        YUV420P,
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
        byte[] mPictureBytes;

        public bool Loaded
        {
            get 
            {
                return mLoaded;
            }
        }

        public int BytesPerPicture
        {
            get 
            {
                switch (mFormat)
                {
                case YUVFileFormat.YUV420I:
                    return (mWidth * mHeight) * (3/2);
                case YUVFileFormat.YUV420P:
                    return (mWidth * mHeight) * (3/2);
                case YUVFileFormat.YUYV:
                    return (mWidth * mHeight * 2);
                case YUVFileFormat.YVYU:
                    return (mWidth * mHeight * 2);
                default:
                    return 0;
                }
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


                /* Now, read the image data and cache it as Y,U,V */
                try
                {
                    mPictureBytes = new byte[this.BytesPerPicture];
                    new_reader.Read(mPictureBytes, 0, this.BytesPerPicture);                    
                    mStream = result_stream;
                    mReader = new_reader;
                    mLoaded = true;
                }
                catch (Exception e)
                {
                    result_stream.Close();
                    mLoaded = false;
                }
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
