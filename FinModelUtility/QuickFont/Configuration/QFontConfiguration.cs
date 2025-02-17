// Decompiled with JetBrains decompiler
// Type: QuickFont.Configuration.QFontConfiguration
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

#nullable disable
namespace QuickFont.Configuration
{
  public class QFontConfiguration
  {
    public QFontShadowConfiguration ShadowConfig;
    public QFontKerningConfiguration KerningConfig = new QFontKerningConfiguration();
    public bool TransformToCurrentOrthogProjection;

    public QFontConfiguration()
    {
    }

    public QFontConfiguration(bool addDropShadow, bool transformToOrthogProjection = false)
    {
      if (addDropShadow)
        this.ShadowConfig = new QFontShadowConfiguration();
      this.TransformToCurrentOrthogProjection = transformToOrthogProjection;
    }
  }
}
