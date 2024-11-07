using System;
using System.Drawing;

namespace csAcq4
{
    public class HisPic
    {
        public IntPtr PtrPicBuf;   //图像数据的指针
        public int Width;  //图像有多少个像素宽/多少列
        public int Height;  //图像有多少个像素高/多少行
        public int PicSize; //图像有多大/多少个字节
        public int PixelSize;   //图像中每个像素占多少个字节

        //构造函数1
        public HisPic(IntPtr ptrPicBuf, int width, int height, int pixelSize)
        {
            this.PtrPicBuf = ptrPicBuf;
            this.Width = width;
            this.Height = height;
            this.PixelSize = pixelSize;
            this.PicSize = width * height * pixelSize;
        }

        //构造函数2，主要用于Form1一开始做一个初始的LivePic
        public HisPic()
        {
            this.PtrPicBuf = IntPtr.Zero;
            this.Width = 512;
            this.Height = 512;
            this.PixelSize = 2;
            this.PicSize = 512 * 512 * 2;
        }

        //主要用于将原始数据HisPic，根据LUT数据，转化为能够在PictureBox中显示出来的24位RGB
        public void TransferFromHisPicTo24RGBBitMap(ref Bitmap bitmap, float lutMin, float lutMax, double width, double height)
        {
            // 如果 bitmap 为空，则跳过处理
            if (bitmap == null)
            {
                // 直接返回，不执行任何操作
                return;
            }

            // 如果 PtrPicBuf 为空，则跳过处理
            if (PtrPicBuf == IntPtr.Zero)
            {
                // 直接返回，不执行任何操作
                return;
            }

            // 如果 PixelSize 不为 1 或 2，则跳过处理
            if (PixelSize != 1 && PixelSize != 2)
            {
                // 直接返回，不执行任何操作
                return;
            }
            // 创建一个数组，用于存放处理后的图像数据
            byte[] tempPicData = new byte[3 * (Int32)height * (Int32)width];

            // 从IntPtr PtrPicBuf中将数据处理后存入tempPicData
            unsafe
            {
                // 定义指针变量
                Byte* PtrPixelData8 = null;
                UInt16* PtrPixelData16 = null;
                UInt16 PixelData = 0;

                // 根据 PixelSize 进行不同的类型转换
                if (PixelSize == 1)
                {
                    // 对于 8bit 图像，使用 Byte 类型读取数据
                    PtrPixelData8 = (Byte*)PtrPicBuf;
                }
                else if (PixelSize == 2)
                {
                    // 对于 16bit 图像，使用 UInt16 类型读取数据
                    PtrPixelData16 = (UInt16*)PtrPicBuf;
                }

                for (int i = 0; i < (height * width - 1); i++)
                {
                    // 显示前矫正
                    if (PixelSize == 1 && PtrPixelData8 != null)
                    {
                        // 对于 8bit 图像
                        PixelData = PtrPixelData8[i];
                    }
                    else if (PixelSize == 2 && PtrPixelData16 != null)
                    {
                        // 对于 16bit 图像
                        PixelData = PtrPixelData16[i];
                    }

                    // 进行坏点校正
                    switch (i % (Int32)width)
                    {
                        case 766:
                            PixelData = PixelSize == 1 ? PtrPixelData8[i - 1] : PtrPixelData16[i - 1];
                            break;
                        case 767:
                            PixelData = PixelSize == 1 ? PtrPixelData8[i + 1] : PtrPixelData16[i + 1];
                            break;
                        case 1534:
                            PixelData = PixelSize == 1 ? PtrPixelData8[i - 1] : PtrPixelData16[i - 1];
                            break;
                        case 1535:
                            PixelData = PixelSize == 1 ? PtrPixelData8[i - 2] : PtrPixelData16[i - 2];
                            break;
                        default:
                            // 默认情况下不做坏点校正
                            PixelData = PixelSize == 1 ? PtrPixelData8[i] : PtrPixelData16[i];
                            break;
                    }

                    // 进行 LUT 映射
                    byte BytePixelData = 0;
                    if (PixelData >= lutMax)
                    {
                        BytePixelData = 255;
                    }
                    else if (PixelData <= lutMin)
                    {
                        BytePixelData = 0;
                    }
                    else
                    {
                        BytePixelData = (byte)(((PixelData - lutMin) / (lutMax - lutMin)) * 255);
                    }

                    // 将灰度值填充为RGB三个通道的值
                    tempPicData[i * 3] = BytePixelData;
                    tempPicData[i * 3 + 1] = BytePixelData;
                    tempPicData[i * 3 + 2] = BytePixelData;
                }
            }

            // 将Bitmap bitmap打开（用LockBits方法）以供修改
            Rectangle hisRec = new Rectangle(0, 0, (Int32)width, (Int32)height);
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(hisRec, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // 将数据从数组byte[] temPicData赋值给Bitmap bitmap后，关上bitmap（用UnlockBits方法）
            System.Runtime.InteropServices.Marshal.Copy(tempPicData, 0, bmpData.Scan0, 3 * (Int32)width * (Int32)height);
            bitmap.UnlockBits(bmpData);
        }


        //===========================================================
        //****************** -1.其他对象和方法 **********************
        //===========================================================

        //测试用的方法，返回图片的第一个像素（占两个字节）的数据
        public int GetFirstPixelValue()
        {
            unsafe
            {
                UInt16* PtrPixelData = (UInt16*)PtrPicBuf;
                UInt16 PixelData = PtrPixelData[0];
                return PixelData;
            }
        }
        public int GetPixelValue(int idx)
        {
            unsafe
            {
                Byte* PtrPixelData = (Byte*)PtrPicBuf;
                Byte PixelData = PtrPixelData[idx];
                return PixelData;
            }
        }


    }
}
