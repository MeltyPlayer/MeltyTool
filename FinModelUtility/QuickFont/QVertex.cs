// Decompiled with JetBrains decompiler
// Type: QuickFont.QVertex
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Mathematics;
using System.Runtime.InteropServices;

#nullable disable
namespace QuickFont
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct QVertex
  {
    public Vector3 Position;
    public Vector2 TextureCoord;
    public Vector4 VertexColor;
  }
}
