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
        public byte[] mY;
        public byte[] mU;
        public byte[] mV;

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
                    break;
                case YUVFileFormat.YUV420P:
                    return (mWidth * mHeight) * (3/2);
                    break;
                case YUVFileFormat.YUYV:
                    return (mWidth * mHeight * 2);
                    break;
                case YUVFileFormat.YVYU:
                    return (mWidth * mHeight * 2);
                    break;
                default:
                    return 0;
                    break;
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

                mStream = result_stream;
                mReader = new_reader;
                mLoaded = true;

                /* Now, read the image data and cache it as Y,U,V, using doubling. */
                byte[] picture_bytes = new byte[this.BytesPerPicture];
                new_reader.Read(picture_bytes, 0, this.BytesPerPicture);
                FillYUVCache(picture_bytes);
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

        void FillYUVCache(byte[] pic_bytes)
        {
            mY = new byte[ mWidth * mHeight ];
            mU = new byte[ mWidth * mHeight ];
            mV = new byte[ mWidth * mHeight ];

            switch (mFormat)
            {
            case YUVFileFormat.YUV420I:
                /* YUV interlaced. Ys first: */
                Array.Copy(pic_bytes, mY, mWidth * mHeight);
                break;
            case YUVFileFormat.YUV420P:
                /* YUV420 planar. First, all the Ys */
                Array.Copy(pic_bytes, mY, mWidth * mHeight);
                /* The rest of the array is UV UV UV UV .. */
                break;
            default:
                break;
            }
        }

    }
}

/* End file */
