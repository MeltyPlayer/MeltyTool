// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontData
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections.Generic;
using System.Drawing;

#nullable disable
namespace QuickFont
{
  internal class QFontData
  {
    public Dictionary<string, int> KerningPairs;
    public TexturePage[] Pages;
    public Dictionary<char, QFontGlyph> CharSetMapping;
    public float MeanGlyphWidth;
    public int MaxGlyphHeight;
    public int MaxLineHeight;
    public QFont DropShadowFont;
    public bool IsDropShadow;
    public bool NaturallyMonospaced;

    public bool IsMonospacingActive(QFontRenderOptions options)
    {
      return options.Monospacing == QFontMonospacing.Natural && this.NaturallyMonospaced || options.Monospacing == QFontMonospacing.Yes;
    }

    public float GetMonoSpaceWidth(QFontRenderOptions options)
    {
      return (float) Math.Ceiling(1.0 + (1.0 + (double) options.CharacterSpacing) * (double) this.MeanGlyphWidth);
    }

    public List<string> Serialize()
    {
      List<string> stringList1 = new List<string>();
      List<string> stringList2 = stringList1;
      int num = this.Pages.Length;
      string str1 = num.ToString() ?? "";
      stringList2.Add(str1);
      List<string> stringList3 = stringList1;
      num = this.CharSetMapping.Count;
      string str2 = num.ToString() ?? "";
      stringList3.Add(str2);
      foreach (KeyValuePair<char, QFontGlyph> keyValuePair in this.CharSetMapping)
      {
        char key = keyValuePair.Key;
        QFontGlyph qfontGlyph = keyValuePair.Value;
        List<string> stringList4 = stringList1;
        string[] strArray = new string[13];
        strArray[0] = key.ToString();
        strArray[1] = " ";
        strArray[2] = qfontGlyph.Page.ToString();
        strArray[3] = " ";
        num = qfontGlyph.Rect.X;
        strArray[4] = num.ToString();
        strArray[5] = " ";
        num = qfontGlyph.Rect.Y;
        strArray[6] = num.ToString();
        strArray[7] = " ";
        num = qfontGlyph.Rect.Width;
        strArray[8] = num.ToString();
        strArray[9] = " ";
        num = qfontGlyph.Rect.Height;
        strArray[10] = num.ToString();
        strArray[11] = " ";
        strArray[12] = qfontGlyph.YOffset.ToString();
        string str3 = string.Concat(strArray);
        stringList4.Add(str3);
      }
      return stringList1;
    }

    public void Deserialize(List<string> input, out int pageCount, out char[] charSet)
    {
      this.CharSetMapping = new Dictionary<char, QFontGlyph>();
      List<char> charList = new List<char>();
      try
      {
        pageCount = int.Parse(input[0]);
        int num = int.Parse(input[1]);
        for (int index = 0; index < num; ++index)
        {
          string[] strArray = input[2 + index].Split(' ');
          QFontGlyph qfontGlyph = new QFontGlyph(int.Parse(strArray[1]), new Rectangle(int.Parse(strArray[2]), int.Parse(strArray[3]), int.Parse(strArray[4]), int.Parse(strArray[5])), int.Parse(strArray[6]), strArray[0][0]);
          this.CharSetMapping.Add(strArray[0][0], qfontGlyph);
          charList.Add(strArray[0][0]);
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Failed to parse qfont file. Invalid format.", ex);
      }
      charSet = charList.ToArray();
    }

    public void CalculateMeanWidth()
    {
      this.MeanGlyphWidth = 0.0f;
      foreach (KeyValuePair<char, QFontGlyph> keyValuePair in this.CharSetMapping)
        this.MeanGlyphWidth += (float) keyValuePair.Value.Rect.Width;
      this.MeanGlyphWidth /= (float) this.CharSetMapping.Count;
    }

    public void CalculateMaxHeight()
    {
      this.MaxGlyphHeight = 0;
      this.MaxLineHeight = 0;
      foreach (KeyValuePair<char, QFontGlyph> keyValuePair in this.CharSetMapping)
      {
        this.MaxGlyphHeight = Math.Max(keyValuePair.Value.Rect.Height, this.MaxGlyphHeight);
        this.MaxLineHeight = Math.Max(keyValuePair.Value.Rect.Height + keyValuePair.Value.YOffset, this.MaxLineHeight);
      }
    }

    public int GetKerningPairCorrection(int index, string text, TextNode textNode)
    {
      if (this.KerningPairs == null)
        return 0;
      char[] chArray = new char[2];
      if (index + 1 == text.Length)
      {
        if (textNode == null || textNode.Next == null || textNode.Next.Type != TextNodeType.Word)
          return 0;
        chArray[1] = textNode.Next.Text[0];
      }
      else
        chArray[1] = text[index + 1];
      chArray[0] = text[index];
      string key = new string(chArray);
      return this.KerningPairs.ContainsKey(key) ? this.KerningPairs[key] : 0;
    }

    public void Dispose()
    {
      foreach (TexturePage page in this.Pages)
        page.Dispose();
      this.DropShadowFont?.Dispose();
    }
  }
}
