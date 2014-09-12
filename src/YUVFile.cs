/* YUVFileAccess.cs */
/* (C) Kynesim Ltd 2014 */

using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace yuv3
{
    public enum YUVFileFormat
    {
        YUV420I,
        YUV420P,
        YUYV,
        YVYU,
        Y8,
        Y16,
        Unknown
    }
    
    public class YUVFile
    {
        public string mFileName;
        public int mWidth;
        public int mHeight;
        public int mFrame;
        public YUVFileFormat mFormat;
        FileStream mStream;
        bool mLoaded;
        byte[] mPictureBytes;
        int mPictureOffset;
        IStatusNotifier mNotifier;

        public int PixelBits
        {
            get
            {
                return (mFormat == YUVFileFormat.Y16) ? 16 : 8;
            }
        }

        public int MaxPixelValue
        {
            get
            {
                return (mFormat == YUVFileFormat.Y16 ? 65535 : 255);
            }
        }

        public int Width
        {
            get
            {
                return mWidth;
            }
        }
        public int Height
        {
            get
            {
                return mHeight;
            }
        }

        public int Frame
        {
            get
            {
                return mFrame;
            }
        }

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
                case YUVFileFormat.Y8:
                    return (mWidth * mHeight);
                case YUVFileFormat.Y16:
                    return (mWidth * mHeight * 2);
                default:
                    return 0;
                }
            }
        }

        public uint Checksum
        {
            get
            {
                uint sum = 0;
                if (mPictureBytes != null)
                {
                    for (int i = 0; i <mPictureBytes.Length; ++i)
                    {
                        sum += mPictureBytes[i];
                    }
                }
                return sum;
            }
        }


        public void CloseFile()
        {
            ClearData();
            if (mStream != null)
            {
                mStream.Close();
                mStream = null;
            }
            mLoaded = false;
        }


      
        unsafe bool Convert420I(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {                    
                int yptr = 0;
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
                        Utils.YUVToRGB(outp, alpha, 
                                 mPictureBytes[yptr], 
                                 mPictureBytes[uptr],
                                 mPictureBytes[vptr]);
                        
                        Utils.YUVToRGB(outp+4, alpha,
                                 mPictureBytes[yptr + 1], 
                                     mPictureBytes[uptr],
                                 mPictureBytes[vptr]);
                        
                        uptr += 2;
                        vptr += 2;
                    }
                    if ((j&1) == 0)
                    {
                        uptr = old_uptr;
                        vptr = old_vptr;
                    }
                }
                return true;
            }


        unsafe bool ConvertY8(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {
                byte *out_line = (byte *)ioData.Scan0.ToPointer();
                int stride = ioData.Stride;

                Parallel.For(0, mHeight, (j) =>
                {
                    int uptr = (j * mWidth);
                    byte *outp = out_line + (j* stride);
                    for (int i =0 ;i < mWidth; ++i, outp += 4, ++uptr)
                    {
                        outp[3] = (byte)alpha;
                        outp[2] = outp[1] = outp[0] = (byte)mPictureBytes[uptr];
                    }
                });
                return true;
            }

        
        unsafe bool ConvertY16(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {
                byte *out_line = (byte *)ioData.Scan0.ToPointer();
                int stride = ioData.Stride;

                Parallel.For(0, mHeight, (j) =>
                {
                    int uptr = (j * mWidth * 2);
                    byte *outp = out_line + (j* stride);
                    for (int i =0 ;i < mWidth; ++i, outp += 4, uptr += 2)
                    {
                        uint y = (uint)mPictureBytes[uptr]| ((uint)(mPictureBytes[uptr+1]) << 8);
                        outp[3] = (byte)alpha;
                        outp[2] = outp[1] = outp[0] = (byte)(y>>8);
                    }
                });
                return true;
            }

            
        unsafe bool ConvertYUYV(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {
                byte *out_line = (byte *)ioData.Scan0.ToPointer();
                int stride = ioData.Stride;

                Parallel.For(0, mHeight, (j) =>
                {
                    // This is YUYV.. 
                    int uptr = (j * (mWidth *2));
                    byte *outp = out_line + (j * stride);
                    for (int i =0 ;i < mWidth; i += 2, outp += 8, uptr += 4)
                    {
                        // First pixel.
                        Utils.YUVToRGB(outp, alpha,
                                 mPictureBytes[uptr], // Y
                                 mPictureBytes[uptr + 1], // U
                                 mPictureBytes[uptr + 3] // V
                            );
                        Utils.YUVToRGB(outp + 4, alpha,
                                 mPictureBytes[uptr + 2], // Y2
                                 mPictureBytes[uptr + 1], // U
                                 mPictureBytes[uptr + 3] // V
                            );
                    }
                });
                return true;
            }

        unsafe bool Convert420P(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {                    
                int base_uptr = (mWidth * mHeight);
                int base_vptr = base_uptr + ((mWidth/2) * (mHeight/2));
                int uvline = (mWidth/2);
                byte *out_line = (byte *)ioData.Scan0.ToPointer();
                int stride = ioData.Stride;

                /* YUV420 interleaved */
                Parallel.For(0, mHeight, (j) => 
                {
                    //Console.WriteLine(String.Format("Line {0} pb {1}", j, mPictureBytes.Length));
                    int uptr, vptr, yptr;
                    byte *outp;
                    uptr = base_uptr + ((j/2) * uvline);
                    vptr = base_vptr + ((j/2) * uvline);
                    yptr = mWidth *j;
                    outp = out_line + (j*stride);
                    for (int i = 0; i < mWidth; i += 2, outp += 8, yptr += 2, ++uptr, ++vptr)
                    {
                        //Console.WriteLine(String.Format("ptr {0}, {1}, {2} -> {3} @ {4}, {5}\n",
                         //                                yptr, uptr, vptr, 
                          //                              mPictureBytes.Length,
                        //                            i,j));

                        Utils.YUVToRGB(outp, alpha, 
                                  mPictureBytes[yptr],  
                                 mPictureBytes[uptr], 
                                 mPictureBytes[vptr]);
                        
                        Utils.YUVToRGB(outp+4, alpha,
                                 mPictureBytes[yptr + 1], 
                                 mPictureBytes[uptr], 
                                 mPictureBytes[vptr]);
                    }
                });
                return true;
            }

        public unsafe bool ToRGB32(System.Drawing.Imaging.BitmapData ioData, int alpha)
            {
                if (!EnsureData())
                {
                    return false;
                }

                
                //Console.WriteLine(String.Format("ToRGB32! {0} {1}", mWidth,mHeight));
                switch (mFormat) 
                {
                case YUVFileFormat.YUV420I:
                    return Convert420I(ioData, alpha);
                case YUVFileFormat.YUV420P:
                    return Convert420P(ioData, alpha);
                case YUVFileFormat.YUYV:
                    return ConvertYUYV(ioData, alpha);
                case YUVFileFormat.Y8:
                    return ConvertY8(ioData, alpha);
                case YUVFileFormat.Y16:
                    return ConvertY16(ioData, alpha);
                default:
                    break;
                }
                return false;
            }

        public void QueryYUV(int x, int y, out int cy, out int cu, out int cv)
        {
            int yp = -1, up = -1, vp = -1;
            switch (mFormat)
            {
            case YUVFileFormat.YUV420I:
                yp = (y * mWidth) + x;
                up = ((mHeight + (y/2)) * mWidth) + (x&~1);
                vp = up +1;
                break;
            case YUVFileFormat.YUV420P:
            {
                int chroma = (mHeight * mWidth);
                yp  = (y *mWidth) + x;
                up = chroma + ((mWidth / 2) * (y/2)) + (x/2);
                vp = chroma + ((mWidth / 2) * (mHeight / 2)) + ((mWidth/2) * (y/2)) + (x/2);
                break;
            }
            case YUVFileFormat.YUYV:
            {
                yp = (y * mWidth * 2) + (x * 2);
                if ((x & 1) != 0)
                {
                    // Odd pixel - U is behind.
                    up = yp  - 1;
                    vp = yp + 1;
                }
                else
                {
                    // Even pixel - U is ahead.
                    up = yp + 1;
                    vp = yp + 3;
                }
                break;
            }
            case YUVFileFormat.Y8:
            {
                /* Magic! */ 
                yp = (y * mWidth) + x;
                cy = mPictureBytes[yp];
                cu = -1; cv = -1;
                return;
            }
            case YUVFileFormat.Y16:
            {
                /* Magic! */
                yp = (y * mWidth * 2) + (x * 2);
                cy = (mPictureBytes[yp]) | (mPictureBytes[yp+1] << 8);
                cu = -1; cv = -1;
                return;
            }
            default:
                break;
            }
            if (yp >= 0 && up >= 0 && vp >= 0)
            {
                cy = mPictureBytes[yp];
                cu = mPictureBytes[up];
                cv = mPictureBytes[vp];
            }
            else
            {
                cy = cu= cv = -1;
            }
        }

        public uint IDFromBytes(byte[] pic_bytes)
        {
            return ((uint)pic_bytes[0] << 24) |
                ((uint)pic_bytes[1] << 16) |
                ((uint)pic_bytes[2] << 8) |
                ((uint)pic_bytes[3]);
        }

        public bool SeekID(uint id, int start_at, out uint frame_number)
        {
            if (mStream == null || !mLoaded)
            {
                // Not loaded.
                frame_number = 0xFFFFFFFF;
                return false;
            }
            // Otherwise.
            byte[] header = new byte[16];
            int frame_no = start_at;
            try 
            {
                while (true)
                {
                    long where = BytesPerPicture * frame_no;
                    mNotifier.Log(String.Format("Seeking id 0x{0:x8} @ frame = {1}, offset = {2}", id, frame_no,
                                                (int)where));
                    mStream.Seek(BytesPerPicture * frame_no, SeekOrigin.Begin);
                    mStream.Read(header, 0, 16);
                    uint cur_id = IDFromBytes(header);
                    if (cur_id == id)
                    {
                        frame_number = (uint)frame_no;
                        mNotifier.Log(String.Format("id 0x{0:x8} found at frame {1}, offset {2}", id, frame_no,
                                                    (int)where));
                        return true;
                    }
                    ++frame_no;
                }
            }
            catch 
            {
                mNotifier.Log(String.Format("Failed to find id 0x{0:x8} in frames 0..{1}", 
                                            id, frame_no-1));
                frame_number = 0xFFFFFFFF;
                return false;
            }
        }

        public bool EnsureData()
        {
            if (FileOffsetOfPicture < 0 || mStream == null || !mLoaded)
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
                    //Console.WriteLine(String.Format("Reading {0} bytes from {1}",
                    //BytesPerPicture, FileOffsetOfPicture));
                    mStream.Seek(FileOffsetOfPicture, SeekOrigin.Begin);
                    mStream.Read(mPictureBytes, 
                                 0, BytesPerPicture);
                    mPictureOffset = FileOffsetOfPicture;
                    //Console.WriteLine(String.Format("Sum = {0}",
                    //Checksum));
                }
                catch (Exception e)
                {
                    mPictureBytes= null;
                    mNotifier.Warning(String.Format("Cannot read image data - {0}", e.ToString()),
                                      false);
                    return false;
                }
            }
            else
            {
                //Console.WriteLine(String.Format("XX {0}", Checksum));
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
                    new FileStream(in_file, FileMode.Open,
                                   FileAccess.Read,
                                   FileShare.Read);

                // Remove any old data.
                ClearData();

                try
                {
                    mStream = result_stream;
                    mLoaded = true;
                }
                catch (Exception e)
                {
                    mNotifier.Warning(String.Format("Cannot open {0} - {1}",
                                                    in_file, e), false);
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

        public bool GetFrameID(out uint frame_id)
        {
            if (mPictureBytes != null && mPictureBytes.Length >= Constants.kYUVHeaderBytes)
            {
                frame_id = IDFromBytes(mPictureBytes);
                return true;
            }
            else
            {
                frame_id = 0xFFFFFFFF;
                return false;
            }
        }

    }
}

/* End file */
