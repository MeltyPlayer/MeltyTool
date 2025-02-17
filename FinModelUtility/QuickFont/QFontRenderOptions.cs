// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontRenderOptions
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Mathematics;
using System.Drawing;

#nullable disable
namespace QuickFont
{
  public class QFontRenderOptions
  {
    public Color Colour = Color.White;
    public float CharacterSpacing = 0.05f;
    public float WordSpacing = 0.9f;
    public float LineSpacing = 1f;
    public bool DropShadowActive;
    public Vector2 DropShadowOffset = new Vector2(0.16f, 0.16f);
    public Color DropShadowColour = Color.FromArgb(128, Color.Black);
    public QFontMonospacing Monospacing;
    public Viewport? TransformToViewport;
    public bool LockToPixel;
    public float LockToPixelRatio = 1f;
    public bool UseDefaultBlendFunction = true;
    public bool WordWrap = true;
    private float _justifyCharWeightForExpand = 0.5f;
    private float _justifyCharWeightForContract = 0.2f;
    public float JustifyCapExpand = 0.5f;
    public float JustifyCapContract = 0.1f;
    public float JustifyContractionPenalty = 2f;
    public Rectangle ClippingRectangle;

    public float DropShadowOpacity
    {
      set
      {
        this.DropShadowColour = Color.FromArgb((int) (byte) ((double) value * (double) byte.MaxValue), Color.Black);
      }
    }

    public float JustifyCharacterWeightForExpand
    {
      get => this._justifyCharWeightForExpand;
      set => this._justifyCharWeightForExpand = MathHelper.Clamp(value, 0.0f, 1f);
    }

    public float JustifyCharacterWeightForContract
    {
      get => this._justifyCharWeightForContract;
      set => this._justifyCharWeightForContract = MathHelper.Clamp(value, 0.0f, 1f);
    }

    public QFontRenderOptions CreateClone()
    {
      return new QFontRenderOptions()
      {
        Colour = this.Colour,
        CharacterSpacing = this.CharacterSpacing,
        WordSpacing = this.WordSpacing,
        LineSpacing = this.LineSpacing,
        DropShadowActive = this.DropShadowActive,
        DropShadowOffset = this.DropShadowOffset,
        DropShadowColour = this.DropShadowColour,
        Monospacing = this.Monospacing,
        TransformToViewport = this.TransformToViewport,
        LockToPixel = this.LockToPixel,
        LockToPixelRatio = this.LockToPixelRatio,
        UseDefaultBlendFunction = this.UseDefaultBlendFunction,
        JustifyCharacterWeightForExpand = this.JustifyCharacterWeightForExpand,
        _justifyCharWeightForExpand = this._justifyCharWeightForExpand,
        JustifyCharacterWeightForContract = this.JustifyCharacterWeightForContract,
        _justifyCharWeightForContract = this._justifyCharWeightForContract,
        JustifyCapExpand = this.JustifyCapExpand,
        JustifyCapContract = this.JustifyCapContract,
        JustifyContractionPenalty = this.JustifyContractionPenalty,
        ClippingRectangle = this.ClippingRectangle
      };
    }
  }
}
