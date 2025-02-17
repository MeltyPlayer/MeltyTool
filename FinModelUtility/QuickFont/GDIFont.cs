// Decompiled with JetBrains decompiler
// Type: QuickFont.GDIFont
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;

#nullable disable
namespace QuickFont
{
  public sealed class GDIFont : IFont, IDisposable
  {
    private Font _font;
    private FontFamily _fontFamily;

    public float Size => this._font.Size;

    public bool HasKerningInformation => false;

    public GDIFont(
      string fontPath,
      float size,
      FontStyle style,
      int superSampleLevels = 1,
      float scale = 1f)
    {
      try
      {
        PrivateFontCollection privateFontCollection = new PrivateFontCollection();
        privateFontCollection.AddFontFile(fontPath);
        this._fontFamily = privateFontCollection.Families[0];
      }
      catch (FileNotFoundException ex)
      {
        this._fontFamily = ((IEnumerable<FontFamily>) new InstalledFontCollection().Families).FirstOrDefault<FontFamily>((Func<FontFamily, bool>) (family => string.Equals(fontPath, family.Name)));
        if (this._fontFamily == null)
          this._fontFamily = SystemFonts.DefaultFont.FontFamily;
      }
      if (!this._fontFamily.IsStyleAvailable(style))
        throw new ArgumentException("Font file: " + fontPath + " does not support style: " + style.ToString());
      this._font = new Font(this._fontFamily, size * scale * (float) superSampleLevels, style);
    }

    public Point DrawString(string s, Graphics graph, Brush color, int x, int y)
    {
      graph.DrawString(s, this._font, color, (float) x, (float) y);
      return Point.Empty;
    }

    public int GetKerning(char c1, char c2)
    {
      throw new NotImplementedException("Font kerning for GDI Fonts is not implemented. Should be calculated manually.");
    }

    public SizeF MeasureString(string s, Graphics graph) => graph.MeasureString(s, this._font);

    public override string ToString() => this._font.Name;

    public void Dispose()
    {
      this._font.Dispose();
      this._fontFamily.Dispose();
    }
  }
}
