YUV3
<rrw@kynesim.co.uk>
2014-09-12
--------------

 Welcome to YUV3.

 You can find YUV3's home page at http://code.google.com/p/yuv3 .

 YUV3 is licenced under the MPL 1.1 .

 YUV3 is the spiritual (and actual) successor to the YUV2 which was
shipped as part of tstools (http://code.google.com/p/tstools).

 It is written in C# and should work on both Linux and Windows. On
Windows you will need the .NET runtime v.4 or above.
        
 DANGER WILL ROBINSON! On Linux you will need the latest
Xamarin packages from http://www.mono-project.org/ , as 
Ubuntu 14.04 (at least) has broken distribution packages.

 Then run:

 make
 mono bin/yuv3.exe

 And you should be away.

 Things you should know:

 - Support for YUV420I and VYUY formats is tentative as I don't have any
   files in these formats.

 - What I mean by the file formats:

    Y8 - Y data only, 8 bpp
    Y16 - Y data only, 16 bpp
    YUV420P  - W*H  Y bytes, followed by (W*H)/4 U bytes followed by (W*H)/4 V bytes.
    YUV420I  - W*H Y bytes followed by (W*H)/4 pairs of UV .
    YUYV     - W*H*2 total bytes in YUYV groups.
    VYUY     - W*H*2 total bytes in VYUY groups.

 - YUV3 can pick up parameters from the file names you give it:
   
   * Width and height are discovered from the first match of the regex
      _([0-9]+)x([0-9]+), so loading 'foo_540x230' will autoset the 
     dimensions to 540x320.

   * Format is found from the file extension:
   
     .yuv420p  - YUV420P
     .yuv420i  - YUV420I
     .yuyv  
     .yuyv422  - YUYV
     .vyuy
     .vyuy422  - VYUY

 - There is a field called 'ID'. If you put 4 bytes of ID (PTS or whatever)
    in the first four bytes of each frame of your YUV file, you can search
    by them. This is useful to align files from different decoders.

    When you hit return in the ID field, we will attempt to search for this ID
   from the next frame onward.

 - YUV3 calculates a checksum for every frame. This is a simple 32-bit 
    sum of all the bytes in a frame, in the order in which they occur in the
    YUV file. It is somewhat useful for deciding if a decoder is decoding the
    same pixels in a different frame, or is just Wrong (tm).

 - There are bugs. Please report them at the Google code page and I will try
   to fix them.

 - Pixel grids only happen when zoom > 4 (to avoid cluttering the display).
 
 - Sorry for the lack of XOR rendering; GDI+ doesn't support it. Boo.


  Any problems, <rrw@kynesim.co.uk> is your friend. Allegedly.

  Many thanks to Kynesim Ltd for supporting this work; http://www.kynesim.co.uk/
 for further details.

  Enjoy :-)


 rrw 2014-09-12
-----------------

   
