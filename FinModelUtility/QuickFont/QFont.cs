// Decompiled with JetBrains decompiler
// Type: QuickFont.QFont
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using QuickFont.Configuration;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

#nullable disable
namespace QuickFont
{
  [DebuggerDisplay("{FontName}")]
  public class QFont : IDisposable
  {
    private bool _disposed;

    public int MaxLineHeight => this.FontData.MaxLineHeight;

    public int MaxGlyphHeight => this.FontData.MaxGlyphHeight;

    internal QFontData FontData { get; private set; }

    public string FontName { get; }

    internal QFont(QFontData fontData) => this.FontData = fontData;

    public QFont(IFont font, QFontBuilderConfiguration config = null)
    {
      this.InitialiseQFont(font, config);
    }

    public QFont(string fontPath, float size, QFontBuilderConfiguration config, FontStyle style = FontStyle.Regular)
    {
      float scale = 1f;
      using (IFont font = QFont.GetFont(fontPath, size, style, config != null ? config.SuperSampleLevels : 1, scale))
      {
        this.FontName = font.ToString();
        this.InitialiseQFont(font, config);
      }
    }

    public QFont(byte[] fontData, float size, QFontBuilderConfiguration config, FontStyle style = FontStyle.Regular)
    {
      float scale = 1f;
      using (IFont font = (IFont) new FreeTypeFont(fontData, size, style, config != null ? config.SuperSampleLevels : 1, scale))
      {
        this.FontName = font.ToString();
        this.InitialiseQFont(font, config);
      }
    }

    public QFont(string qfontPath, QFontConfiguration loaderConfig, float downSampleFactor = 1f)
    {
      float num = 1f;
      this.InitialiseQFont((IFont) null, new QFontBuilderConfiguration(loaderConfig), Builder.LoadQFontDataFromFile(qfontPath, downSampleFactor * num, loaderConfig));
      this.FontName = qfontPath;
    }

    private void InitialiseQFont(IFont font, QFontBuilderConfiguration config, QFontData data = null)
    {
      this.FontData = data ?? QFont.BuildFont(font, config, (string) null);
      if (this.FontData.Pages.Length != 1 || this.FontData.DropShadowFont != null && this.FontData.DropShadowFont.FontData.Pages.Length != 1)
        throw new NotSupportedException("The implementation of QFontDrawing does not support multiple textures per Font/Shadow. Thus this font can not be properly rendered in all cases. Reduce number of characters or increase QFontBuilderConfiguration.MaxTexSize QFontShadowConfiguration.PageMaxTextureSize to contain all characters/char-shadows in one Bitmap=>Texture.");
    }

    public static void CreateTextureFontFiles(
      IFont font,
      string newFontName,
      QFontBuilderConfiguration config)
    {
      Builder.SaveQFontDataToFile(QFont.BuildFont(font, config, newFontName), newFontName);
    }

    public static void CreateTextureFontFiles(
      string fileName,
      float size,
      string newFontName,
      QFontBuilderConfiguration config,
      FontStyle style = FontStyle.Regular)
    {
      using (IFont font = QFont.GetFont(fileName, size, style, config != null ? config.SuperSampleLevels : 1))
        QFont.CreateTextureFontFiles(font, newFontName, config);
    }

    private static IFont GetFont(
      string fontPath,
      float size,
      FontStyle style,
      int superSampleLevels = 1,
      float scale = 1f)
    {
      return !File.Exists(fontPath) ? (IFont) new GDIFont(fontPath, size, style, superSampleLevels, scale) : (IFont) new FreeTypeFont(fontPath, size, style, superSampleLevels, scale);
    }

    private static QFontData BuildFont(
      IFont font,
      QFontBuilderConfiguration config,
      string saveName)
    {
      return new Builder(font, config).BuildFontData(saveName);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      if (disposing)
        this.FontData.Dispose();
      this._disposed = true;
    }

    public SizeF Measure(string text, SizeF maxSize, QFontAlignment alignment)
    {
      return new QFontDrawingPrimitive(this).Measure(text, maxSize, alignment);
    }

    public SizeF Measure(string text, float maxWidth, QFontAlignment alignment)
    {
      return new QFontDrawingPrimitive(this).Measure(text, maxWidth, alignment);
    }

    public SizeF Measure(ProcessedText processedText)
    {
      return new QFontDrawingPrimitive(this).Measure(processedText);
    }

    public SizeF Measure(string text, QFontAlignment alignment = QFontAlignment.Left)
    {
      return new QFontDrawingPrimitive(this).Measure(text, alignment);
    }
  }
}
