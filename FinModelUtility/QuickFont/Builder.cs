// Decompiled with JetBrains decompiler
// Type: QuickFont.Builder
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using QuickFont.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

#nullable disable
namespace QuickFont
{
  internal class Builder
  {
    private string _charSet;
    private QFontBuilderConfiguration _config;
    private IFont _font;

    public Builder(IFont font, QFontBuilderConfiguration config)
    {
      this._charSet = config.CharSet;
      this._config = config;
      this._font = font;
    }

    private static Dictionary<char, QFontGlyph> CreateCharGlyphMapping(QFontGlyph[] glyphs)
    {
      Dictionary<char, QFontGlyph> charGlyphMapping = new Dictionary<char, QFontGlyph>();
      for (int index = 0; index < glyphs.Length; ++index)
        charGlyphMapping.Add(glyphs[index].Character, glyphs[index]);
      return charGlyphMapping;
    }

    private List<SizeF> GetGlyphSizes(IFont font)
    {
      int num = 5 + (int) (0.1 * (double) font.Size);
      Bitmap bitmap = new Bitmap(512, 512, PixelFormat.Format24bppRgb);
      Graphics graph = Graphics.FromImage((Image) bitmap);
      List<SizeF> glyphSizes = new List<SizeF>();
      foreach (char ch in this._charSet)
      {
        SizeF sizeF = font.MeasureString(ch.ToString() ?? "", graph);
        glyphSizes.Add(new SizeF(sizeF.Width + (float) num, sizeF.Height + (float) num));
      }
      graph.Dispose();
      bitmap.Dispose();
      return glyphSizes;
    }

    private SizeF GetMaxGlyphSize(List<SizeF> sizes)
    {
      SizeF maxGlyphSize = new SizeF(0.0f, 0.0f);
      for (int index = 0; index < this._charSet.Length; ++index)
      {
        SizeF siz = sizes[index];
        if ((double) siz.Width > (double) maxGlyphSize.Width)
        {
          ref SizeF local = ref maxGlyphSize;
          siz = sizes[index];
          double width = (double) siz.Width;
          local.Width = (float) width;
        }
        siz = sizes[index];
        if ((double) siz.Height > (double) maxGlyphSize.Height)
        {
          ref SizeF local = ref maxGlyphSize;
          siz = sizes[index];
          double height = (double) siz.Height;
          local.Height = (float) height;
        }
      }
      return maxGlyphSize;
    }

    private SizeF GetMinGlyphSize(List<SizeF> sizes)
    {
      SizeF minGlyphSize = new SizeF(float.MaxValue, float.MaxValue);
      for (int index = 0; index < this._charSet.Length; ++index)
      {
        SizeF siz = sizes[index];
        if ((double) siz.Width < (double) minGlyphSize.Width)
        {
          ref SizeF local = ref minGlyphSize;
          siz = sizes[index];
          double width = (double) siz.Width;
          local.Width = (float) width;
        }
        siz = sizes[index];
        if ((double) siz.Height < (double) minGlyphSize.Height)
        {
          ref SizeF local = ref minGlyphSize;
          siz = sizes[index];
          double height = (double) siz.Height;
          local.Height = (float) height;
        }
      }
      return minGlyphSize;
    }

    private bool IsMonospaced(List<SizeF> sizes)
    {
      SizeF minGlyphSize = this.GetMinGlyphSize(sizes);
      SizeF maxGlyphSize = this.GetMaxGlyphSize(sizes);
      return (double) maxGlyphSize.Width - (double) minGlyphSize.Width < (double) maxGlyphSize.Width * 0.05000000074505806;
    }

    private Bitmap CreateInitialBitmap(
      IFont font,
      SizeF maxSize,
      int initialMargin,
      out QFontGlyph[] glyphs,
      TextGenerationRenderHint renderHint)
    {
      glyphs = new QFontGlyph[this._charSet.Length];
      Bitmap initialBitmap = new Bitmap(((int) Math.Ceiling((double) maxSize.Width) + 2 * initialMargin) * this._charSet.Length, (int) Math.Ceiling((double) maxSize.Height) + 2 * initialMargin, PixelFormat.Format24bppRgb);
      Graphics graphics = Graphics.FromImage((Image) initialBitmap);
      switch (renderHint)
      {
        case TextGenerationRenderHint.AntiAliasGridFit:
          graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
          break;
        case TextGenerationRenderHint.AntiAlias:
          graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
          break;
        case TextGenerationRenderHint.SizeDependent:
          graphics.TextRenderingHint = (double) font.Size <= 12.0 ? TextRenderingHint.ClearTypeGridFit : TextRenderingHint.AntiAlias;
          break;
        case TextGenerationRenderHint.ClearTypeGridFit:
          graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
          break;
        case TextGenerationRenderHint.SystemDefault:
          graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
          break;
      }
      graphics.CompositingQuality = CompositingQuality.HighQuality;
      graphics.SmoothingMode = SmoothingMode.HighQuality;
      graphics.CompositingMode = CompositingMode.SourceOver;
      int num = initialMargin;
      for (int index = 0; index < this._charSet.Length; ++index)
      {
        IFont font1 = font;
        char ch = this._charSet[index];
        string s1 = ch.ToString() ?? "";
        Graphics graph1 = graphics;
        Brush white = Brushes.White;
        int x = num;
        int y = initialMargin;
        Point point = font1.DrawString(s1, graph1, white, x, y);
        IFont font2 = font;
        ch = this._charSet[index];
        string s2 = ch.ToString() ?? "";
        Graphics graph2 = graphics;
        SizeF sizeF = font2.MeasureString(s2, graph2);
        glyphs[index] = new QFontGlyph(0, new Rectangle(num - initialMargin + point.X, initialMargin + point.Y, (int) sizeF.Width + initialMargin * 2, (int) sizeF.Height + initialMargin * 2), 0, this._charSet[index]);
        num += (int) sizeF.Width + initialMargin * 2;
      }
      graphics.Flush();
      graphics.Dispose();
      return initialBitmap;
    }

    private static void RetargetGlyphRectangleInwards(
      BitmapData bitmapData,
      QFontGlyph glyph,
      bool setYOffset,
      byte alphaTolerance)
    {
      Rectangle rect = glyph.Rect;
      Builder.EmptyDel emptyDel = bitmapData.PixelFormat != PixelFormat.Format32bppArgb ? (Builder.EmptyDel) ((data, x, y) => QBitmap.EmptyPixel(data, x, y)) : (Builder.EmptyDel) ((data, x, y) => QBitmap.EmptyAlphaPixel(data, x, y, alphaTolerance));
      int x1;
      for (x1 = rect.X; x1 < bitmapData.Width; ++x1)
      {
        for (int y = rect.Y; y <= rect.Y + rect.Height; ++y)
        {
          if (!emptyDel(bitmapData, x1, y))
            goto label_7;
        }
      }
label_7:
      int x2;
      for (x2 = rect.X + rect.Width; x2 >= 0; --x2)
      {
        for (int y = rect.Y; y <= rect.Y + rect.Height; ++y)
        {
          if (!emptyDel(bitmapData, x2, y))
            goto label_14;
        }
      }
label_14:
      int y1;
      for (y1 = rect.Y; y1 < bitmapData.Height; ++y1)
      {
        for (int x3 = x1; x3 <= x2; ++x3)
        {
          if (!emptyDel(bitmapData, x3, y1))
            goto label_21;
        }
      }
label_21:
      int y2;
      for (y2 = rect.Y + rect.Height; y2 >= 0; --y2)
      {
        for (int x4 = x1; x4 <= x2; ++x4)
        {
          if (!emptyDel(bitmapData, x4, y2))
            goto label_28;
        }
      }
label_28:
      if (y2 < y1)
        y1 = y2 = rect.Y;
      if (x2 < x1)
        x1 = x2 = rect.X;
      glyph.Rect = new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
      if (!setYOffset)
        return;
      glyph.YOffset = glyph.Rect.Y;
    }

    private static void RetargetGlyphRectangleOutwards(
      BitmapData bitmapData,
      QFontGlyph glyph,
      bool setYOffset,
      byte alphaTolerance)
    {
      Rectangle rect = glyph.Rect;
      Builder.EmptyDel emptyDel = bitmapData.PixelFormat != PixelFormat.Format32bppArgb ? (Builder.EmptyDel) ((data, x, y) => QBitmap.EmptyPixel(data, x, y)) : (Builder.EmptyDel) ((data, x, y) => QBitmap.EmptyAlphaPixel(data, x, y, alphaTolerance));
      int x1;
      for (x1 = rect.X; x1 >= 0; --x1)
      {
        bool flag = false;
        for (int y = rect.Y; y <= rect.Y + rect.Height; ++y)
        {
          if (!emptyDel(bitmapData, x1, y))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
        {
          ++x1;
          break;
        }
      }
      int x2;
      for (x2 = rect.X + rect.Width; x2 < bitmapData.Width; ++x2)
      {
        bool flag = false;
        for (int y = rect.Y; y <= rect.Y + rect.Height; ++y)
        {
          if (!emptyDel(bitmapData, x2, y))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
        {
          --x2;
          break;
        }
      }
      int y1;
      for (y1 = rect.Y; y1 >= 0; --y1)
      {
        bool flag = false;
        for (int x3 = x1; x3 <= x2; ++x3)
        {
          if (!emptyDel(bitmapData, x3, y1))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
        {
          ++y1;
          break;
        }
      }
      int y2;
      for (y2 = rect.Y + rect.Height; y2 < bitmapData.Height; ++y2)
      {
        bool flag = false;
        for (int x4 = x1; x4 <= x2; ++x4)
        {
          if (!emptyDel(bitmapData, x4, y2))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
        {
          --y2;
          break;
        }
      }
      glyph.Rect = new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
      if (!setYOffset)
        return;
      glyph.YOffset = glyph.Rect.Y;
    }

    private static List<QBitmap> GenerateBitmapSheetsAndRepack(
      QFontGlyph[] sourceGlyphs,
      BitmapData[] sourceBitmaps,
      int destSheetWidth,
      int destSheetHeight,
      out QFontGlyph[] destGlyphs,
      int destMargin)
    {
      List<QBitmap> bitmapSheetsAndRepack = new List<QBitmap>();
      destGlyphs = new QFontGlyph[sourceGlyphs.Length];
      QBitmap qbitmap = (QBitmap) null;
      int val2 = 0;
      foreach (QFontGlyph sourceGlyph in sourceGlyphs)
        val2 = Math.Max(sourceGlyph.Rect.Height, val2);
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      for (int index1 = 0; index1 < 2; ++index1)
      {
        bool flag = index1 == 0;
        int num4 = 0;
        int num5 = 0;
        int val1 = 0;
        int num6 = 0;
        for (int index2 = 0; index2 < sourceGlyphs.Length; ++index2)
        {
          if (!flag && qbitmap == null)
          {
            if (num1 == bitmapSheetsAndRepack.Count)
            {
              qbitmap = new QBitmap(new Bitmap(Math.Min(destSheetWidth, num2), Math.Min(destSheetHeight, num3), PixelFormat.Format32bppArgb));
              qbitmap.Clear32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 0);
            }
            else
            {
              qbitmap = new QBitmap(new Bitmap(destSheetWidth, destSheetHeight, PixelFormat.Format32bppArgb));
              qbitmap.Clear32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 0);
            }
            bitmapSheetsAndRepack.Add(qbitmap);
          }
          ++num6;
          if (num6 > 10 * sourceGlyphs.Length)
            throw new Exception("Failed to fit font into texture pages");
          Rectangle rect = sourceGlyphs[index2].Rect;
          if (num4 + rect.Width + 2 * destMargin <= destSheetWidth && num5 + rect.Height + 2 * destMargin <= destSheetHeight)
          {
            if (!flag)
            {
              if (sourceBitmaps[sourceGlyphs[index2].Page].PixelFormat == PixelFormat.Format32bppArgb)
                QBitmap.Blit(sourceBitmaps[sourceGlyphs[index2].Page], qbitmap.BitmapData, rect.X, rect.Y, rect.Width, rect.Height, num4 + destMargin, num5 + destMargin);
              else
                QBitmap.BlitMask(sourceBitmaps[sourceGlyphs[index2].Page], qbitmap.BitmapData, rect.X, rect.Y, rect.Width, rect.Height, num4 + destMargin, num5 + destMargin);
              destGlyphs[index2] = new QFontGlyph(bitmapSheetsAndRepack.Count - 1, new Rectangle(num4 + destMargin, num5 + destMargin, rect.Width, rect.Height), sourceGlyphs[index2].YOffset, sourceGlyphs[index2].Character);
            }
            else
            {
              num2 = Math.Max(num2, num4 + rect.Width + 2 * destMargin);
              num3 = Math.Max(num3, num5 + rect.Height + 2 * destMargin);
            }
            num4 += rect.Width + 2 * destMargin;
            val1 = Math.Max(val1, rect.Height);
          }
          else if (num4 + rect.Width + 2 * destMargin > destSheetWidth)
          {
            --index2;
            num5 += val1 + 2 * destMargin;
            num4 = 0;
            if (num5 + val2 + 2 * destMargin > destSheetHeight)
            {
              num5 = 0;
              if (!flag)
              {
                qbitmap = (QBitmap) null;
              }
              else
              {
                num2 = 0;
                num3 = 0;
                ++num1;
              }
            }
          }
        }
      }
      return bitmapSheetsAndRepack;
    }

    public QFontData BuildFontData(string saveName = null)
    {
      if (this._config.SuperSampleLevels <= 0 || this._config.SuperSampleLevels > 8)
        throw new ArgumentOutOfRangeException("SuperSampleLevels = [" + this._config.SuperSampleLevels.ToString() + "] is an unsupported value. Please use values in the range [1,8]");
      int initialMargin = 3;
      int destMargin = this._config.GlyphMargin * this._config.SuperSampleLevels;
      List<SizeF> glyphSizes = this.GetGlyphSizes(this._font);
      QFontGlyph[] glyphs;
      Bitmap initialBitmap = this.CreateInitialBitmap(this._font, this.GetMaxGlyphSize(glyphSizes), initialMargin, out glyphs, this._config.TextGenerationRenderHint);
      BitmapData bitmapData = initialBitmap.LockBits(new Rectangle(0, 0, initialBitmap.Width, initialBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
      int val1 = int.MaxValue;
      foreach (QFontGlyph glyph in glyphs)
      {
        Builder.RetargetGlyphRectangleInwards(bitmapData, glyph, true, this._config.KerningConfig.AlphaEmptyPixelTolerance);
        val1 = Math.Min(val1, glyph.YOffset);
      }
      int num = val1 - 1;
      foreach (QFontGlyph qfontGlyph in glyphs)
        qfontGlyph.YOffset -= num;
      Size optimalPageSize = Builder.GetOptimalPageSize(initialBitmap.Width * this._config.SuperSampleLevels, initialBitmap.Height * this._config.SuperSampleLevels, this._config.PageMaxTextureSize);
      QFontGlyph[] destGlyphs;
      List<QBitmap> bitmapSheetsAndRepack = Builder.GenerateBitmapSheetsAndRepack(glyphs, new BitmapData[1]
      {
        bitmapData
      }, optimalPageSize.Width, optimalPageSize.Height, out destGlyphs, destMargin);
      initialBitmap.UnlockBits(bitmapData);
      initialBitmap.Dispose();
      if (this._config.SuperSampleLevels != 1)
      {
        Builder.ScaleSheetsAndGlyphs(bitmapSheetsAndRepack, destGlyphs, 1f / (float) this._config.SuperSampleLevels);
        Builder.RetargetAllGlyphs(bitmapSheetsAndRepack, destGlyphs, this._config.KerningConfig.AlphaEmptyPixelTolerance);
      }
      List<TexturePage> texturePageList = new List<TexturePage>();
      foreach (QBitmap qbitmap in bitmapSheetsAndRepack)
        texturePageList.Add(new TexturePage(qbitmap.BitmapData));
      QFontData qfontData = new QFontData()
      {
        CharSetMapping = Builder.CreateCharGlyphMapping(destGlyphs),
        Pages = texturePageList.ToArray()
      };
      qfontData.CalculateMeanWidth();
      qfontData.CalculateMaxHeight();
      qfontData.KerningPairs = KerningCalculator.CalculateKerning(this._charSet.ToCharArray(), destGlyphs, bitmapSheetsAndRepack, this._config.KerningConfig, this._font);
      qfontData.NaturallyMonospaced = this.IsMonospaced(glyphSizes);
      if (saveName != null)
      {
        if (bitmapSheetsAndRepack.Count == 1)
        {
          bitmapSheetsAndRepack[0].Bitmap.UnlockBits(bitmapSheetsAndRepack[0].BitmapData);
          bitmapSheetsAndRepack[0].Bitmap.Save(saveName + ".png", ImageFormat.Png);
          bitmapSheetsAndRepack[0] = new QBitmap(bitmapSheetsAndRepack[0].Bitmap);
        }
        else
        {
          for (int index = 0; index < bitmapSheetsAndRepack.Count; ++index)
          {
            bitmapSheetsAndRepack[index].Bitmap.UnlockBits(bitmapSheetsAndRepack[index].BitmapData);
            bitmapSheetsAndRepack[index].Bitmap.Save(saveName + "_sheet_" + index.ToString() + ".png", ImageFormat.Png);
            bitmapSheetsAndRepack[index] = new QBitmap(bitmapSheetsAndRepack[index].Bitmap);
          }
        }
      }
      if (this._config.ShadowConfig != null)
        qfontData.DropShadowFont = Builder.BuildDropShadow(bitmapSheetsAndRepack, destGlyphs, this._config.ShadowConfig, this._charSet.ToCharArray(), this._config.KerningConfig.AlphaEmptyPixelTolerance);
      foreach (QBitmap qbitmap in bitmapSheetsAndRepack)
        qbitmap.Free();
      char[] chArray = Builder.FirstIntercept(qfontData.CharSetMapping);
      if (chArray != null)
        throw new Exception("Failed to create glyph set. Glyphs '" + chArray[0].ToString() + "' and '" + chArray[1].ToString() + "' were overlapping. This is could be due to an error in the font, or a bug in Graphics.MeasureString().");
      return qfontData;
    }

    private static Size GetOptimalPageSize(int width, int height, int pageMaxTextureSize)
    {
      int num = width / pageMaxTextureSize + 1;
      return new Size(pageMaxTextureSize, num * height);
    }

    private static QFont BuildDropShadow(
      List<QBitmap> sourceFontSheets,
      QFontGlyph[] sourceFontGlyphs,
      QFontShadowConfiguration shadowConfig,
      char[] charSet,
      byte alphaTolerance)
    {
      List<BitmapData> bitmapDataList = new List<BitmapData>();
      foreach (QBitmap sourceFontSheet in sourceFontSheets)
        bitmapDataList.Add(sourceFontSheet.BitmapData);
      QFontGlyph[] destGlyphs;
      List<QBitmap> bitmapSheetsAndRepack = Builder.GenerateBitmapSheetsAndRepack(sourceFontGlyphs, bitmapDataList.ToArray(), shadowConfig.PageMaxTextureSize, shadowConfig.PageMaxTextureSize, out destGlyphs, shadowConfig.GlyphMargin + shadowConfig.BlurRadius * 3);
      if ((double) Math.Abs(shadowConfig.Scale - 1f) > 1.4012984643248171E-45)
        Builder.ScaleSheetsAndGlyphs(bitmapSheetsAndRepack, destGlyphs, shadowConfig.Scale);
      foreach (QBitmap qbitmap in bitmapSheetsAndRepack)
      {
        qbitmap.Colour32(byte.MaxValue, byte.MaxValue, byte.MaxValue);
        if (shadowConfig.Type == ShadowType.Blurred)
          qbitmap.BlurAlpha(shadowConfig.BlurRadius, shadowConfig.BlurPasses);
        else
          qbitmap.ExpandAlpha(shadowConfig.BlurRadius, shadowConfig.BlurPasses);
      }
      Builder.RetargetAllGlyphs(bitmapSheetsAndRepack, destGlyphs, alphaTolerance);
      List<TexturePage> texturePageList = new List<TexturePage>();
      foreach (QBitmap qbitmap in bitmapSheetsAndRepack)
        texturePageList.Add(new TexturePage(qbitmap.BitmapData));
      QFontData fontData = new QFontData()
      {
        CharSetMapping = new Dictionary<char, QFontGlyph>()
      };
      for (int index = 0; index < charSet.Length; ++index)
        fontData.CharSetMapping.Add(charSet[index], destGlyphs[index]);
      fontData.Pages = texturePageList.ToArray();
      fontData.CalculateMeanWidth();
      fontData.CalculateMaxHeight();
      foreach (QBitmap qbitmap in bitmapSheetsAndRepack)
        qbitmap.Free();
      fontData.IsDropShadow = true;
      return new QFont(fontData);
    }

    private static void ScaleSheetsAndGlyphs(List<QBitmap> pages, QFontGlyph[] glyphs, float scale)
    {
      foreach (QBitmap page in pages)
        page.DownScale32((int) ((double) page.Bitmap.Width * (double) scale), (int) ((double) page.Bitmap.Height * (double) scale));
      foreach (QFontGlyph glyph in glyphs)
      {
        glyph.Rect = new Rectangle((int) ((double) glyph.Rect.X * (double) scale), (int) ((double) glyph.Rect.Y * (double) scale), (int) ((double) glyph.Rect.Width * (double) scale), (int) ((double) glyph.Rect.Height * (double) scale));
        glyph.YOffset = (int) ((double) glyph.YOffset * (double) scale);
      }
    }

    private static void RetargetAllGlyphs(
      List<QBitmap> pages,
      QFontGlyph[] glyphs,
      byte alphaTolerance)
    {
      foreach (QFontGlyph glyph in glyphs)
        Builder.RetargetGlyphRectangleOutwards(pages[glyph.Page].BitmapData, glyph, false, alphaTolerance);
    }

    public static void SaveQFontDataToFile(QFontData data, string filePath)
    {
      List<string> stringList = data.Serialize();
      StreamWriter streamWriter = new StreamWriter(filePath + ".qfont");
      foreach (string str in stringList)
        streamWriter.WriteLine(str);
      streamWriter.Close();
    }

    public static void CreateBitmapPerGlyph(
      QFontGlyph[] sourceGlyphs,
      QBitmap[] sourceBitmaps,
      out QFontGlyph[] destGlyphs,
      out QBitmap[] destBitmaps)
    {
      destBitmaps = new QBitmap[sourceGlyphs.Length];
      destGlyphs = new QFontGlyph[sourceGlyphs.Length];
      for (int page = 0; page < sourceGlyphs.Length; ++page)
      {
        QFontGlyph sourceGlyph = sourceGlyphs[page];
        destGlyphs[page] = new QFontGlyph(page, new Rectangle(0, 0, sourceGlyph.Rect.Width, sourceGlyph.Rect.Height), sourceGlyph.YOffset, sourceGlyph.Character);
        destBitmaps[page] = new QBitmap(new Bitmap(sourceGlyph.Rect.Width, sourceGlyph.Rect.Height, PixelFormat.Format32bppArgb));
        QBitmap.Blit(sourceBitmaps[sourceGlyph.Page].BitmapData, destBitmaps[page].BitmapData, sourceGlyph.Rect, 0, 0);
      }
    }

    public static QFontData LoadQFontDataFromFile(
      string filePath,
      float downSampleFactor,
      QFontConfiguration loaderConfig)
    {
      List<string> input = new List<string>();
      StreamReader streamReader = new StreamReader(filePath);
      string str1;
      while ((str1 = streamReader.ReadLine()) != null)
        input.Add(str1);
      streamReader.Close();
      QFontData qfontData = new QFontData();
      int pageCount;
      char[] charSet;
      qfontData.Deserialize(input, out pageCount, out charSet);
      string str2 = filePath.Replace(".qfont", "").Replace(" ", "");
      List<QBitmap> qbitmapList = new List<QBitmap>();
      if (pageCount == 1)
      {
        qbitmapList.Add(new QBitmap(str2 + ".png"));
      }
      else
      {
        for (int index = 0; index < pageCount; ++index)
          qbitmapList.Add(new QBitmap(str2 + "_sheet_" + index.ToString()));
      }
      foreach (QFontGlyph glyph in qfontData.CharSetMapping.Values)
        Builder.RetargetGlyphRectangleOutwards(qbitmapList[glyph.Page].BitmapData, glyph, false, loaderConfig.KerningConfig.AlphaEmptyPixelTolerance);
      char[] chArray1 = Builder.FirstIntercept(qfontData.CharSetMapping);
      if (chArray1 != null)
        throw new Exception("Failed to load font from file. Glyphs '" + chArray1[0].ToString() + "' and '" + chArray1[1].ToString() + "' were overlapping. If you are texturing your font without locking pixel opacity, then consider using a larger glyph margin. This can be done by setting QFontBuilderConfiguration myQfontBuilderConfig.GlyphMargin, and passing it into CreateTextureFontFiles.");
      if ((double) downSampleFactor > 1.0)
      {
        foreach (QBitmap qbitmap in qbitmapList)
          qbitmap.DownScale32((int) ((double) qbitmap.Bitmap.Width * (double) downSampleFactor), (int) ((double) qbitmap.Bitmap.Height * (double) downSampleFactor));
        foreach (QFontGlyph qfontGlyph in qfontData.CharSetMapping.Values)
        {
          qfontGlyph.Rect = new Rectangle((int) ((double) qfontGlyph.Rect.X * (double) downSampleFactor), (int) ((double) qfontGlyph.Rect.Y * (double) downSampleFactor), (int) ((double) qfontGlyph.Rect.Width * (double) downSampleFactor), (int) ((double) qfontGlyph.Rect.Height * (double) downSampleFactor));
          qfontGlyph.YOffset = (int) ((double) qfontGlyph.YOffset * (double) downSampleFactor);
        }
      }
      else if ((double) downSampleFactor < 1.0)
      {
        QFontGlyph[] destGlyphs1;
        QBitmap[] destBitmaps;
        Builder.CreateBitmapPerGlyph(Helper.ToArray<QFontGlyph>((ICollection<QFontGlyph>) qfontData.CharSetMapping.Values), qbitmapList.ToArray(), out destGlyphs1, out destBitmaps);
        for (int index = 0; index < destGlyphs1.Length; ++index)
        {
          QBitmap qbitmap = destBitmaps[index];
          qbitmap.DownScale32(Math.Max((int) ((double) qbitmap.Bitmap.Width * (double) downSampleFactor), 1), Math.Max((int) ((double) qbitmap.Bitmap.Height * (double) downSampleFactor), 1));
          destGlyphs1[index].Rect = new Rectangle(0, 0, qbitmap.Bitmap.Width, qbitmap.Bitmap.Height);
          destGlyphs1[index].YOffset = (int) ((double) destGlyphs1[index].YOffset * (double) downSampleFactor);
        }
        BitmapData[] sourceBitmaps = new BitmapData[destBitmaps.Length];
        for (int index = 0; index < destBitmaps.Length; ++index)
          sourceBitmaps[index] = destBitmaps[index].BitmapData;
        int destSheetWidth = (int) ((double) qbitmapList[0].Bitmap.Width * (0.10000000149011612 + (double) downSampleFactor));
        int destSheetHeight = (int) ((double) qbitmapList[0].Bitmap.Height * (0.10000000149011612 + (double) downSampleFactor));
        for (int index = 0; index < pageCount; ++index)
          qbitmapList[index].Free();
        QFontGlyph[] destGlyphs2;
        qbitmapList = Builder.GenerateBitmapSheetsAndRepack(destGlyphs1, sourceBitmaps, destSheetWidth, destSheetHeight, out destGlyphs2, 4);
        qfontData.CharSetMapping = Builder.CreateCharGlyphMapping(destGlyphs2);
        foreach (QBitmap qbitmap in destBitmaps)
          qbitmap.Free();
        pageCount = qbitmapList.Count;
      }
      qfontData.Pages = new TexturePage[pageCount];
      for (int index = 0; index < pageCount; ++index)
        qfontData.Pages[index] = new TexturePage(qbitmapList[index].BitmapData);
      if ((double) Math.Abs(downSampleFactor - 1f) > 1.4012984643248171E-45)
      {
        foreach (QFontGlyph glyph in qfontData.CharSetMapping.Values)
          Builder.RetargetGlyphRectangleOutwards(qbitmapList[glyph.Page].BitmapData, glyph, false, loaderConfig.KerningConfig.AlphaEmptyPixelTolerance);
        char[] chArray2 = Builder.FirstIntercept(qfontData.CharSetMapping);
        if (chArray2 != null)
          throw new Exception("Failed to load font from file. Glyphs '" + chArray2[0].ToString() + "' and '" + chArray2[1].ToString() + "' were overlapping. This occurred only after resizing your texture font, implying that there is a bug in QFont. ");
      }
      List<QFontGlyph> qfontGlyphList = new List<QFontGlyph>();
      foreach (char key in charSet)
        qfontGlyphList.Add(qfontData.CharSetMapping[key]);
      if (loaderConfig.ShadowConfig != null)
        qfontData.DropShadowFont = Builder.BuildDropShadow(qbitmapList, qfontGlyphList.ToArray(), loaderConfig.ShadowConfig, Helper.ToArray<char>((ICollection<char>) charSet), loaderConfig.KerningConfig.AlphaEmptyPixelTolerance);
      qfontData.KerningPairs = KerningCalculator.CalculateKerning(Helper.ToArray<char>((ICollection<char>) charSet), qfontGlyphList.ToArray(), qbitmapList, loaderConfig.KerningConfig);
      qfontData.CalculateMeanWidth();
      qfontData.CalculateMaxHeight();
      for (int index = 0; index < pageCount; ++index)
        qbitmapList[index].Free();
      return qfontData;
    }

    private static char[] FirstIntercept(Dictionary<char, QFontGlyph> charSet)
    {
      char[] array = Helper.ToArray<char>((ICollection<char>) charSet.Keys);
      for (int index1 = 0; index1 < array.Length; ++index1)
      {
        for (int index2 = index1 + 1; index2 < array.Length; ++index2)
        {
          if (charSet[array[index1]].Page == charSet[array[index2]].Page && Builder.RectangleIntersect(charSet[array[index1]].Rect, charSet[array[index2]].Rect))
            return new char[2]
            {
              array[index1],
              array[index2]
            };
        }
      }
      return (char[]) null;
    }

    private static bool RectangleIntersect(Rectangle r1, Rectangle r2)
    {
      return r1.X < r2.X + r2.Width && r1.X + r1.Width > r2.X && r1.Y < r2.Y + r2.Height && r1.Y + r1.Height > r2.Y;
    }

    public static int PowerOfTwo(int x)
    {
      int num1 = 0;
      uint num2 = (uint) x;
      if (x < 0)
        return 0;
      while (num2 > 0U)
      {
        num2 >>= 1;
        ++num1;
      }
      uint num3 = (uint) (1 << num1 - 1);
      if ((long) num3 < (long) x)
        num3 <<= 1;
      return (int) num3;
    }

    private delegate bool EmptyDel(BitmapData data, int x, int y);
  }
}
