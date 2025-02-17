// Decompiled with JetBrains decompiler
// Type: QuickFont.QBitmap
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Drawing;
using System.Drawing.Imaging;

#nullable disable
namespace QuickFont
{
  public class QBitmap
  {
    public Bitmap Bitmap;
    public BitmapData BitmapData;

    public QBitmap(string filePath) => this.LockBits(new Bitmap(filePath));

    public QBitmap(Bitmap bitmap) => this.LockBits(bitmap);

    private void LockBits(Bitmap bitmap)
    {
      this.Bitmap = bitmap;
      this.BitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
    }

    public unsafe void Clear32(byte r, byte g, byte b, byte a)
    {
      byte* scan0 = (byte*) (void*) this.BitmapData.Scan0;
      for (int index1 = 0; index1 < this.BitmapData.Height; ++index1)
      {
        for (int index2 = 0; index2 < this.BitmapData.Width; ++index2)
        {
          *scan0 = b;
          scan0[1] = g;
          scan0[2] = r;
          scan0[3] = a;
          scan0 += 4;
        }
        scan0 += this.BitmapData.Stride - this.BitmapData.Width * 4;
      }
    }

    public static unsafe bool EmptyPixel(BitmapData bitmapData, int px, int py)
    {
      if (px < 0 || py < 0 || px >= bitmapData.Width || py >= bitmapData.Height)
        return true;
      byte* numPtr = (byte*) ((IntPtr) (void*) bitmapData.Scan0 + bitmapData.Stride * py + px * 3);
      return *numPtr == (byte) 0 && numPtr[1] == (byte) 0 && numPtr[2] == (byte) 0;
    }

    public static unsafe bool EmptyAlphaPixel(
      BitmapData bitmapData,
      int px,
      int py,
      byte alphaEmptyPixelTolerance)
    {
      return px < 0 || py < 0 || px >= bitmapData.Width || py >= bitmapData.Height || (int) *(byte*) ((IntPtr) (void*) bitmapData.Scan0 + bitmapData.Stride * py + px * 4 + 3) <= (int) alphaEmptyPixelTolerance;
    }

    public static unsafe void BlitMask(
      BitmapData source,
      BitmapData target,
      int srcPx,
      int srcPy,
      int srcW,
      int srcH,
      int px,
      int py)
    {
      int num1 = 3;
      int num2 = 4;
      int num3 = Math.Max(px, 0);
      int num4 = Math.Min(px + srcW, target.Width);
      int num5 = Math.Max(py, 0);
      int num6 = Math.Min(py + srcH, target.Height);
      int num7 = num4 - num3;
      int num8 = num5;
      int num9 = num6 - num8;
      if (num7 < 0 || num9 < 0)
        return;
      int num10 = srcPx + num3 - px;
      int num11 = srcPy + num5 - py;
      byte* scan0 = (byte*) (void*) source.Scan0;
      byte* numPtr1 = (byte*) ((IntPtr) (void*) target.Scan0 + num5 * target.Stride);
      byte* numPtr2 = scan0 + num11 * source.Stride;
      int num12 = 0;
      while (num12 < num9)
      {
        byte* numPtr3 = numPtr1 + num3 * num2;
        byte* numPtr4 = numPtr2 + num10 * num1;
        int num13 = 0;
        while (num13 < num7)
        {
          int num14 = ((int) *numPtr4 + (int) numPtr4[1] + (int) numPtr4[2]) / 3;
          if (num14 > (int) byte.MaxValue)
            num14 = (int) byte.MaxValue;
          numPtr3[3] = (byte) num14;
          ++num13;
          numPtr3 += num2;
          numPtr4 += num1;
        }
        ++num12;
        numPtr1 += target.Stride;
        numPtr2 += source.Stride;
      }
    }

    public static void Blit(
      BitmapData source,
      BitmapData target,
      Rectangle sourceRect,
      int px,
      int py)
    {
      QBitmap.Blit(source, target, sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height, px, py);
    }

    public static unsafe void Blit(
      BitmapData source,
      BitmapData target,
      int srcPx,
      int srcPy,
      int srcW,
      int srcH,
      int destX,
      int destY)
    {
      int num1 = 4;
      int num2 = Math.Max(destX, 0);
      int num3 = Math.Min(destX + srcW, target.Width);
      int num4 = Math.Max(destY, 0);
      int num5 = Math.Min(destY + srcH, target.Height);
      int num6 = num3 - num2;
      int num7 = num4;
      int num8 = num5 - num7;
      if (num6 < 0 || num8 < 0)
        return;
      int num9 = srcPx + num2 - destX;
      int num10 = srcPy + num4 - destY;
      byte* scan0 = (byte*) (void*) source.Scan0;
      byte* numPtr1 = (byte*) ((IntPtr) (void*) target.Scan0 + num4 * target.Stride);
      byte* numPtr2 = scan0 + num10 * source.Stride;
      int num11 = 0;
      while (num11 < num8)
      {
        byte* numPtr3 = numPtr1 + num2 * num1;
        byte* numPtr4 = numPtr2 + num9 * num1;
        int num12 = 0;
        while (num12 < num6 * num1)
        {
          *numPtr3 = *numPtr4;
          ++num12;
          ++numPtr3;
          ++numPtr4;
        }
        ++num11;
        numPtr1 += target.Stride;
        numPtr2 += source.Stride;
      }
    }

    public unsafe void PutPixel32(int px, int py, byte r, byte g, byte b, byte a)
    {
      IntPtr num = (IntPtr) (void*) this.BitmapData.Scan0 + this.BitmapData.Stride * py + px * 4;
      *(sbyte*) num = (sbyte) b;
      *(sbyte*) (num + 1) = (sbyte) g;
      *(sbyte*) (num + 2) = (sbyte) r;
      *(sbyte*) (num + 3) = (sbyte) a;
    }

    public unsafe void GetPixel32(
      int px,
      int py,
      ref byte r,
      ref byte g,
      ref byte b,
      ref byte a)
    {
      byte* numPtr = (byte*) ((IntPtr) (void*) this.BitmapData.Scan0 + this.BitmapData.Stride * py + px * 4);
      b = *numPtr;
      g = numPtr[1];
      r = numPtr[2];
      a = numPtr[3];
    }

    public unsafe void PutAlpha32(int px, int py, byte a)
    {
      *(sbyte*) ((IntPtr) (void*) this.BitmapData.Scan0 + this.BitmapData.Stride * py + px * 4 + 3) = (sbyte) a;
    }

    public unsafe void GetAlpha32(int px, int py, ref byte a)
    {
      a = *(byte*) ((IntPtr) (void*) this.BitmapData.Scan0 + this.BitmapData.Stride * py + px * 4 + 3);
    }

    public void DownScale32(int newWidth, int newHeight)
    {
      QBitmap qbitmap = new QBitmap(new Bitmap(newWidth, newHeight, this.Bitmap.PixelFormat));
      if (this.Bitmap.PixelFormat != PixelFormat.Format32bppArgb)
        throw new Exception("DownsScale32 only works on 32 bit images");
      float num1 = (float) this.BitmapData.Width / (float) newWidth;
      float num2 = (float) this.BitmapData.Height / (float) newHeight;
      byte r1 = 0;
      byte g1 = 0;
      byte b1 = 0;
      byte a1 = 0;
      float num3 = num1 * num2;
      for (int py = 0; py < newHeight; ++py)
      {
        for (int px = 0; px < newWidth; ++px)
        {
          float val1_1 = (float) px * num1;
          float val1_2 = (float) (px + 1) * num1;
          float val1_3 = (float) py * num2;
          float val1_4 = (float) (py + 1) * num2;
          int num4 = (int) val1_1;
          int num5 = (int) val1_2;
          int num6 = (int) val1_3;
          int num7 = (int) val1_4;
          if (num4 < 0)
            num4 = 0;
          if (num6 < 0)
            num6 = 0;
          if (num5 >= this.BitmapData.Width)
            num5 = this.BitmapData.Width - 1;
          if (num7 >= this.BitmapData.Height)
            num7 = this.BitmapData.Height - 1;
          float num8 = 0.0f;
          float num9 = 0.0f;
          float num10 = 0.0f;
          float num11 = 0.0f;
          float num12 = 0.0f;
          for (int index1 = num6; index1 <= num7; ++index1)
          {
            for (int index2 = num4; index2 <= num5; ++index2)
            {
              float num13 = Math.Max(val1_1, (float) index2);
              double num14 = (double) Math.Min(val1_2, (float) (index2 + 1));
              float num15 = Math.Max(val1_3, (float) index1);
              float num16 = Math.Min(val1_4, (float) (index1 + 1));
              double num17 = (double) num13;
              float num18 = (float) ((num14 - num17) * ((double) num16 - (double) num15));
              this.GetPixel32(index2, index1, ref r1, ref g1, ref b1, ref a1);
              num11 += num18 * (float) a1;
              if (a1 != (byte) 0)
              {
                num8 += num18 * (float) r1;
                num9 += num18 * (float) g1;
                num10 += num18 * (float) b1;
                num12 += num18;
              }
            }
          }
          float r2 = num8 / num12;
          float g2 = num9 / num12;
          float b2 = num10 / num12;
          float a2 = num11 / num3;
          if ((double) r2 < 0.0)
            r2 = 0.0f;
          if ((double) g2 < 0.0)
            g2 = 0.0f;
          if ((double) b2 < 0.0)
            b2 = 0.0f;
          if ((double) a2 < 0.0)
            a2 = 0.0f;
          if ((double) r2 >= 256.0)
            r2 = (float) byte.MaxValue;
          if ((double) g2 >= 256.0)
            g2 = (float) byte.MaxValue;
          if ((double) b2 >= 256.0)
            b2 = (float) byte.MaxValue;
          if ((double) a2 >= 256.0)
            a2 = (float) byte.MaxValue;
          qbitmap.PutPixel32(px, py, (byte) r2, (byte) g2, (byte) b2, (byte) a2);
        }
      }
      this.Free();
      this.Bitmap = qbitmap.Bitmap;
      this.BitmapData = qbitmap.BitmapData;
    }

    public unsafe void Colour32(byte r, byte g, byte b)
    {
      for (int index1 = 0; index1 < this.BitmapData.Width; ++index1)
      {
        for (int index2 = 0; index2 < this.BitmapData.Height; ++index2)
        {
          IntPtr num = (IntPtr) (void*) this.BitmapData.Scan0 + this.BitmapData.Stride * index2 + index1 * 4;
          *(sbyte*) num = (sbyte) b;
          *(sbyte*) (num + 1) = (sbyte) g;
          *(sbyte*) (num + 2) = (sbyte) r;
        }
      }
    }

    public void ExpandAlpha(int radius, int passes)
    {
      QBitmap qbitmap = new QBitmap(new Bitmap(this.Bitmap.Width, this.Bitmap.Height, this.Bitmap.PixelFormat));
      byte a1 = 0;
      int width = this.Bitmap.Width;
      int height = this.Bitmap.Height;
      for (int index1 = 0; index1 < passes; ++index1)
      {
        for (int py = 0; py < height; ++py)
        {
          for (int px1 = 0; px1 < width; ++px1)
          {
            byte a2 = 0;
            for (int index2 = -radius; index2 <= radius; ++index2)
            {
              int px2 = px1 + index2;
              if (px2 >= 0 && px2 < width)
              {
                this.GetAlpha32(px2, py, ref a1);
                if ((int) a1 > (int) a2)
                  a2 = a1;
              }
            }
            qbitmap.PutAlpha32(px1, py, a2);
          }
        }
        for (int px = 0; px < width; ++px)
        {
          for (int py1 = 0; py1 < height; ++py1)
          {
            byte a3 = 0;
            for (int index3 = -radius; index3 <= radius; ++index3)
            {
              int py2 = py1 + index3;
              if (py2 >= 0 && py2 < height)
              {
                qbitmap.GetAlpha32(px, py2, ref a1);
                if ((int) a1 > (int) a3)
                  a3 = a1;
              }
            }
            this.PutAlpha32(px, py1, a3);
          }
        }
      }
      qbitmap.Free();
    }

    public void BlurAlpha(int radius, int passes)
    {
      QBitmap qbitmap = new QBitmap(new Bitmap(this.Bitmap.Width, this.Bitmap.Height, this.Bitmap.PixelFormat));
      byte a1 = 0;
      int width = this.Bitmap.Width;
      int height = this.Bitmap.Height;
      for (int index1 = 0; index1 < passes; ++index1)
      {
        for (int py = 0; py < height; ++py)
        {
          for (int px1 = 0; px1 < width; ++px1)
          {
            int num1;
            int num2 = num1 = 0;
            for (int index2 = -radius; index2 <= radius; ++index2)
            {
              int px2 = px1 + index2;
              if (px2 >= 0 && px2 < width)
              {
                this.GetAlpha32(px2, py, ref a1);
                num2 += (int) a1;
                ++num1;
              }
            }
            int a2 = num2 / num1;
            qbitmap.PutAlpha32(px1, py, (byte) a2);
          }
        }
        for (int px = 0; px < width; ++px)
        {
          for (int py1 = 0; py1 < height; ++py1)
          {
            int num3;
            int num4 = num3 = 0;
            for (int index3 = -radius; index3 <= radius; ++index3)
            {
              int py2 = py1 + index3;
              if (py2 >= 0 && py2 < height)
              {
                qbitmap.GetAlpha32(px, py2, ref a1);
                num4 += (int) a1;
                ++num3;
              }
            }
            int a3 = num4 / num3;
            this.PutAlpha32(px, py1, (byte) a3);
          }
        }
      }
      qbitmap.Free();
    }

    public void Free()
    {
      this.Bitmap.UnlockBits(this.BitmapData);
      this.Bitmap.Dispose();
    }
  }
}
