// Decompiled with JetBrains decompiler
// Type: QuickFont.IFont
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Drawing;

#nullable disable
namespace QuickFont
{
  public interface IFont : IDisposable
  {
    SizeF MeasureString(string s, Graphics graph);

    float Size { get; }

    bool HasKerningInformation { get; }

    Point DrawString(string s, Graphics graph, Brush color, int x, int y);

    int GetKerning(char c1, char c2);
  }
}
