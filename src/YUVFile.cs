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
        public int mFrame;
        public YUVFileFormat mFormat;
        BinaryReader mReader;
        FileStream mStream;
        bool mLoaded;
        byte[] mPictureBytes;
        int mPictureOffset;
        IStatusNotifier mNotifier;

        public bool Loaded
        {
            get 
            {
                return mLoaded;
            }
        }

        public int FileOffsetOfPicture
        {
            get
            {
                return mFrame * BytesPerPicture;
            }
        }

        public int BytesPerPicture
        {
            get 
            {
                switch (mFormat)
                {
                case YUVFileFormat.YUV420I:
                    return (mWidth * mHeight * 3) / 2;
                case YUVFileFormat.YUV420P:
                    return (mWidth * mHeight * 3) / 2;
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
            ClearData();
            if (mReader != null)
            {
                mReader.Close();
                mReader = null;
            }
            if (mStream != null)
            {
                mStream.Close();
                mStream = null;
            }
            mLoaded = false;
        }

        public unsafe int clamp(int v)
            {
                if  (v < 0)
                {
                    return 0;
                }
                if (v > 255)
                {
                    return 255;
                }
                return v;
            }

        public unsafe void YUVToRGB(byte * result, int alpha,
                             int y, int u, int v)
        {           
            // Alpha
            int r,g ,b;
            result[3] = (byte)alpha;
            // R
            r = (int)(1.164*((float)y-16.0) + 1.596*((float)v-128.0));
            // G
            g = (int)(1.164*((float)y-16.0) - 0.813*((float)v-128.0) - 0.391*((float)u-128.0));
            // B
            b = (int)(1.164*((float)y-16.0) + 2.018*((float)u - 128.0));
            // *result = (alpha << 24) | (r << 16) | (g << 8) | b;

            r = clamp(r);
            g = clamp(g);
            b = clamp(b);

            result[2] = (byte)r;
            result[1] = (byte)g;
            result[0] = (byte)b;

            //result[0] = 0x80;
            //result[1] = 0x60;
            //result[2] = 0xFF;
            //result[3] = 0x20;
        }
      
        unsafe bool Convert420I(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {                    
                int yptr = 0;
                int rgbptr = 0;
                int uptr = (mWidth * mHeight);
                int vptr = uptr + 1;
                int old_uptr = uptr;
                int old_vptr = vptr;
                byte *out_line = (byte *)ioData.Scan0.ToPointer();
                byte *outp = out_line;
                /* YUV420 interleaved */
                for (int j = 0; j < mHeight; ++j, out_line += ioData.Stride)
                {
                    //Console.WriteLine(String.Format("Line {0} pb {1}", j, mPictureBytes.Length));
                    old_uptr = uptr;
                    old_vptr = vptr;
                    outp = out_line;
                    for (int i = 0; i < mWidth; i += 2, outp += 8)
                    {
                        //  Console.WriteLine(String.Format("ptr {0}, {1}, {2} -> {3} @ {4}, {5}\n",
                        //                                 yptr, uptr, vptr, 
                        //                                mPictureBytes.Length,
                        //                               i,j));
                        YUVToRGB(outp, alpha, 
                                 mPictureBytes[yptr], 
                                 mPictureBytes[uptr],
                                 mPictureBytes[vptr]);
                        
                        YUVToRGB(outp+4, alpha,
                                 mPictureBytes[yptr + 1], 
                                     mPictureBytes[uptr],
                                 mPictureBytes[vptr]);
                        
                        ++uptr;
                        ++vptr;
                    }
                    if ((j&1) == 0)
                    {
                        uptr = old_uptr;
                        vptr = old_vptr;
                    }
                }
                return true;
            }


         unsafe bool Convert420P(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {                    
                int yptr = 0;
                int rgbptr = 0;
                int uptr = (mWidth * mHeight);
                int vptr = uptr + ((mWidth/2) * (mHeight/2));
                int old_uptr = uptr;
                int old_vptr = vptr;
                byte *out_line = (byte *)ioData.Scan0.ToPointer();
                byte *outp = out_line;
                /* YUV420 interleaved */
                for (int j = 0; j < mHeight; ++j, out_line += ioData.Stride)
                {
                    //Console.WriteLine(String.Format("Line {0} pb {1}", j, mPictureBytes.Length));
                    old_uptr = uptr;
                    old_vptr = vptr;
                    outp = out_line;
                    for (int i = 0; i < mWidth; i += 2, outp += 8, yptr += 2)
                    {
                        //Console.WriteLine(String.Format("ptr {0}, {1}, {2} -> {3} @ {4}, {5}\n",
                         //                                yptr, uptr, vptr, 
                          //                              mPictureBytes.Length,
                        //                            i,j));

                        YUVToRGB(outp, alpha, 
                                  mPictureBytes[yptr],  
                                 mPictureBytes[uptr], 
                                 mPictureBytes[vptr]);
                        
                        YUVToRGB(outp+4, alpha,
                                 mPictureBytes[yptr + 1], 
                                 mPictureBytes[uptr], 
                                 mPictureBytes[vptr]);
                        
                        ++uptr;
                        ++vptr;
                    }
                    if ((j&1) == 0)
                    {
                        uptr = old_uptr;
                        vptr = old_vptr;
                    }
                }
                return true;
            }

        public unsafe bool ToRGB32(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {
                if (!EnsureData())
                {
                    return false;
                }

                
                switch (mFormat)
                {
                case YUVFileFormat.YUV420I:
                    return Convert420I(ioData, alpha);
                case YUVFileFormat.YUV420P:
                    return Convert420P(ioData, alpha);
                default:
                    break;
                }
                return false;
            }

        public bool EnsureData()
        {
            if (FileOffsetOfPicture < 0)
            {
                ClearData();
                return false;
            }

            if (mPictureBytes == null ||
                mPictureOffset != FileOffsetOfPicture ||
                mPictureBytes.Length < BytesPerPicture)
            {
                try
                {
                    mPictureBytes = new byte[BytesPerPicture];
                    mStream.Seek(FileOffsetOfPicture, SeekOrigin.Begin);
                    mReader.Read(mPictureBytes, 
                                 0, BytesPerPicture);
                }
                catch (Exception e)
                {
                    mPictureBytes= null;
                    mNotifier.Warning(String.Format("Cannot read image data - {0}", e.ToString()),
                                      false);
                    return false;
                }
            }
            return true;
        }

        public bool HasData()
        {
            return mPictureBytes != null;
        }

        public void ClearData()
        {
            mPictureBytes = null;
        }

        public void LoadFile(string in_file)
        {
            if (in_file != null)
            {
                FileStream result_stream = 
                    new FileStream(in_file, FileMode.Open);
                BinaryReader new_reader = 
                    new BinaryReader(result_stream);

                // Remove any old data.
                ClearData();

                try
                {
                    mStream = result_stream;
                    mReader = new_reader;
                    mLoaded = true;
                }
                catch (Exception e)
                {
                    mNotifier.Warning(String.Format("Cannot open {0} - {1}",
                                                    in_file, e), false);
                    if (new_reader != null)
                    {
                        new_reader.Close();    
                    }
                    if (result_stream != null)
                    {
                        result_stream.Close();
                    }
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

        public void Set(int w, int h, int frame, YUVFileFormat fmt)
        {
            mWidth = w;
            mHeight = h;
            mFormat = fmt;
            mFrame = frame;
            // No need to ClearData() - we could reuse it.
        }
        
        public void ReplaceNotifier(IStatusNotifier inNotifier)
        {
            mNotifier = inNotifier;
        }

        public YUVFile(IStatusNotifier inNotifier)
        {
            mLoaded = false;
            mNotifier = inNotifier;
        }
    }
}

/* End file */
