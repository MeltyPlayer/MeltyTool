// Decompiled with JetBrains decompiler
// Type: QuickFont.Helper
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;

#nullable disable
namespace QuickFont
{
  internal static class Helper
  {
    public static T[] ToArray<T>(ICollection<T> collection)
    {
      T[] array = new T[collection.Count];
      collection.CopyTo(array, 0);
      return array;
    }

    public static void SafeGLEnable(EnableCap cap, Action code)
    {
      int num = GL.IsEnabled(cap) ? 1 : 0;
      GL.Enable(cap);
      code();
      if (num != 0)
        return;
      GL.Disable(cap);
    }

    public static void SafeGLEnable(EnableCap[] caps, Action code)
    {
      bool[] flagArray = new bool[caps.Length];
      for (int index = 0; index < caps.Length; ++index)
      {
        if (GL.IsEnabled(caps[index]))
          flagArray[index] = true;
        else
          GL.Enable(caps[index]);
      }
      code();
      for (int index = 0; index < caps.Length; ++index)
      {
        if (!flagArray[index])
          GL.Disable(caps[index]);
      }
    }

    public static int ToRgba(Color color)
    {
      return (int) color.A << 24 | (int) color.B << 16 | (int) color.G << 8 | (int) color.R;
    }

    public static Vector4 ToVector4(Color color)
    {
      return new Vector4()
      {
        X = (float) color.R / (float) byte.MaxValue,
        Y = (float) color.G / (float) byte.MaxValue,
        Z = (float) color.B / (float) byte.MaxValue,
        W = (float) color.A / (float) byte.MaxValue
      };
    }
  }
}
