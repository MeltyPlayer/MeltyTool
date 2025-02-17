// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontSharedState
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Graphics.OpenGL4;

#nullable disable
namespace QuickFont
{
  public class QFontSharedState
  {
    public QFontSharedState(TextureUnit defaultTextureUnit, ShaderLocations shaderVariables)
    {
      this.DefaultTextureUnit = defaultTextureUnit;
      this.ShaderVariables = shaderVariables;
    }

    public TextureUnit DefaultTextureUnit { get; }

    public ShaderLocations ShaderVariables { get; }
  }
}
