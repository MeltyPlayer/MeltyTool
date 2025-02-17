// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontGlyph
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System.Diagnostics;
using System.Drawing;

#nullable disable
namespace QuickFont
{
  [DebuggerDisplay("{Character} Pg:{Page}")]
  public class QFontGlyph
  {
    public int Page;
    public Rectangle Rect;
    public int YOffset;
    public char Character;

    public QFontGlyph(int page, Rectangle rect, int yOffset, char character)
    {
      this.Page = page;
      this.Rect = rect;
      this.YOffset = yOffset;
      this.Character = character;
    }
  }
}
