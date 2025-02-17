// Decompiled with JetBrains decompiler
// Type: QuickFont.TexturePage
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

#nullable disable
namespace QuickFont
{
  internal class TexturePage : IDisposable
  {
    private int _textureID;

    public int TextureID => this._textureID;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public TexturePage(string filePath)
    {
      QBitmap qbitmap = new QBitmap(filePath);
      this.CreateTexture(qbitmap.BitmapData);
      qbitmap.Free();
    }

    public TexturePage(BitmapData dataSource) => this.CreateTexture(dataSource);

    private void CreateTexture(BitmapData dataSource)
    {
      this.Width = dataSource.Width;
      this.Height = dataSource.Height;
      Helper.SafeGLEnable(EnableCap.Texture2D, (Action) (() =>
      {
        GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
        GL.GenTextures(1, out this._textureID);
        GL.BindTexture(TextureTarget.Texture2D, this.TextureID);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, 33069);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, 33069);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 9729);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, 9729);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, this.Width, this.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, dataSource.Scan0);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
      }));
    }

    private static byte[] ConvertBgraToRgba(BitmapData dataSource)
    {
      int length = dataSource.Stride * dataSource.Height;
      byte[] destination = new byte[length];
      Marshal.Copy(dataSource.Scan0, destination, 0, length);
      for (int index = 0; index < destination.Length; index += 4)
      {
        byte num = destination[index];
        destination[index] = destination[index + 2];
        destination[index + 2] = num;
      }
      return destination;
    }

    public void Dispose() => GL.DeleteTexture(this.TextureID);
  }
}
