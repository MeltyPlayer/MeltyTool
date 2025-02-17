// Decompiled with JetBrains decompiler
// Type: QuickFont.ViewportHelper
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

#nullable disable
namespace QuickFont
{
  public static class ViewportHelper
  {
    private static Viewport? _currentViewport;

    public static Viewport? CurrentViewport
    {
      get
      {
        if (!ViewportHelper._currentViewport.HasValue)
          ViewportHelper.UpdateCurrentViewport();
        return ViewportHelper._currentViewport;
      }
    }

    public static void UpdateCurrentViewport()
    {
      int[] data = new int[4];
      GL.GetInteger(GetPName.Viewport, data);
      ViewportHelper._currentViewport = new Viewport?(new Viewport((float) data[0], (float) data[1], (float) data[2], (float) data[3]));
    }

    public static void InvalidateViewport() => ViewportHelper._currentViewport = new Viewport?();

    public static bool IsOrthographicProjection(ref Matrix4 mat)
    {
      return (double) mat.M12 == 0.0 && (double) mat.M13 == 0.0 && (double) mat.M14 == 0.0 && (double) mat.M21 == 0.0 && (double) mat.M23 == 0.0 && (double) mat.M24 == 0.0 && (double) mat.M31 == 0.0 && (double) mat.M32 == 0.0 && (double) mat.M34 == 0.0 && (double) mat.M44 == 1.0;
    }
  }
}
