using System.Drawing;

using fin.language.equations.fixedFunction;
using fin.model;
using fin.util.asserts;

namespace gx;

public partial class GxFixedFunctionMaterial {
  private class ValueManager {
    private readonly IColorValue colorUndefined_;
    private readonly IScalarValue alphaUndefined_;

    private readonly IFixedFunctionEquations<FixedFunctionSource> equations_;
    private readonly IFixedFunctionRegisters registers_;

    private readonly Dictionary<GxCc, IColorValue>
        colorValues_ = new();

    private readonly Dictionary<GxCa, IScalarValue>
        alphaValues_ = new();

    public ValueManager(
        IFixedFunctionEquations<FixedFunctionSource> equations,
        IFixedFunctionRegisters registers) {
      this.equations_ = equations;
      this.registers_ = registers;

      var colorZero = equations.CreateColorConstant(0);
      var colorOne = equations.CreateColorConstant(1);

      this.colorValues_[GxCc.GX_CC_ZERO] = colorZero;
      this.colorValues_[GxCc.GX_CC_ONE] = colorOne;

      this.alphaValues_[GxCa.GX_CA_ZERO] =
          equations.CreateScalarConstant(0);

      this.colorUndefined_ =
          equations.CreateOrGetColorInput(FixedFunctionSource.UNDEFINED);
      this.alphaUndefined_ =
          equations.CreateOrGetScalarInput(FixedFunctionSource.UNDEFINED);
    }

    public void UpdateColorRegister(
        ColorRegister colorRegister,
        IColorValue colorValue) {
      var source = colorRegister switch {
          ColorRegister.GX_TEVPREV => GxCc.GX_CC_CPREV,
          ColorRegister.GX_TEVREG0 => GxCc.GX_CC_C0,
          ColorRegister.GX_TEVREG1 => GxCc.GX_CC_C1,
          ColorRegister.GX_TEVREG2 => GxCc.GX_CC_C2,
          _ => throw new ArgumentOutOfRangeException(
              nameof(colorRegister),
              colorRegister,
              null)
      };

      this.colorValues_[source] = colorValue;
    }

    public void UpdateAlphaRegister(
        ColorRegister alphaRegister,
        IScalarValue alphaValue) {
      var source = alphaRegister switch {
          ColorRegister.GX_TEVPREV => GxCa.GX_CA_APREV,
          ColorRegister.GX_TEVREG0 => GxCa.GX_CA_A0,
          ColorRegister.GX_TEVREG1 => GxCa.GX_CA_A1,
          ColorRegister.GX_TEVREG2 => GxCa.GX_CA_A2,
          _ => throw new ArgumentOutOfRangeException(
              nameof(alphaRegister),
              alphaRegister,
              null)
      };

      this.alphaValues_[source] = alphaValue;
    }

    private readonly IColorValue?[] textureColors_ = new IColorValue?[8];
    private readonly IColorValue?[] textureAlphas_ = new IColorValue?[8];
    private int? textureIndex_ = null;

    public void UpdateTextureIndex(int? index) {
      this.textureIndex_ = index;

      if (index != null) {
        Asserts.True(index >= 0 && index < 8);
      }
    }

    private IColorValue GetTextureColorChannel_() {
      var indexOrNull = this.textureIndex_;

      IColorValue texture;
      if (indexOrNull == null && !STRICT) {
        texture = colorUndefined_;
      } else {
        Asserts.Nonnull(indexOrNull);

        var index = indexOrNull.Value;
        Asserts.True(index >= 0 && index < 8);

        texture = this.textureColors_[index];
        if (texture == null) {
          this.textureColors_[index] =
              texture = this.equations_.CreateOrGetColorInput(
                  FixedFunctionSource.TEXTURE_COLOR_0 + index);
        }
      }

      return texture;
    }

    private IColorValue GetTextureAlphaChannel_() {
      var indexOrNull = this.textureIndex_;

      IColorValue texture;
      if (indexOrNull == null && !STRICT) {
        texture = colorUndefined_;
      } else {
        Asserts.Nonnull(indexOrNull);

        var index = indexOrNull.Value;
        Asserts.True(index >= 0 && index < 8);

        texture = this.textureAlphas_[index];
        if (texture == null) {
          this.textureAlphas_[index] =
              texture = this.equations_.CreateOrGetColorInput(
                  FixedFunctionSource.TEXTURE_ALPHA_0 + index);
        }
      }

      return texture;
    }

    private IScalarValue GetTextureAlphaChannelAsAlpha_() {
      var indexOrNull = this.textureIndex_;

      IScalarValue texture;
      if (indexOrNull == null && !STRICT) {
        texture = alphaUndefined_;
      } else {
        Asserts.Nonnull(indexOrNull);

        var index = indexOrNull.Value;
        Asserts.True(index >= 0 && index < 8);

        texture =
            this.equations_.CreateOrGetScalarInput(
                FixedFunctionSource.TEXTURE_ALPHA_0 + index);
      }

      return texture;
    }


    private GxColorChannel? colorChannel_;

    public void UpdateRascChannel(GxColorChannel? colorChannel) {
      this.colorChannel_ = colorChannel;
    }

    private readonly Dictionary<GxColorChannel, IColorValue>
        colorChannelColorColors_ = new();

    private readonly Dictionary<GxColorChannel, IColorValue>
        colorChannelColorAlphas_ = new();

    private readonly Dictionary<GxColorChannel, IScalarValue>
        colorChannelAlphas_ = new();

    public void UpdateColorChannelColor(
        GxColorChannel colorChannel,
        IColorValue colorValue) {
      this.colorChannelColorColors_[colorChannel] = colorValue;
    }

    public void UpdateColorChannelAlpha(
        GxColorChannel colorChannel,
        IScalarValue alphaValue) {
      this.colorChannelAlphas_[colorChannel] = alphaValue;
      this.colorChannelColorAlphas_[colorChannel] =
          this.equations_.CreateColor(alphaValue);
    }

    private IColorValue GetVertexColorChannel_(GxCc colorSource) {
      var channelOrNull = this.colorChannel_;
      Asserts.Nonnull(channelOrNull);

      var channel = channelOrNull.Value;
      var color = colorSource switch {
          GxCc.GX_CC_RASC => colorChannelColorColors_[channel],
          GxCc.GX_CC_RASA => colorChannelColorAlphas_[channel],
          _               => throw new NotImplementedException()
      };

      return color;
    }

    // TODO: Switch from vertex alpha to ambient/diffuse lights when applicable
    private IScalarValue GetVertexAlphaChannel_() {
      var channelOrNull = this.colorChannel_;
      Asserts.Nonnull(channelOrNull);

      var channel = channelOrNull.Value;
      var alpha = colorChannelAlphas_[channel];

      return alpha;
    }

    private IList<IColorRegister> colorRegisterColors_;
    private IList<Color> konstColorImpls_;

    public void SetColorRegisters(IList<IColorRegister> colorRegisterColors) {
      this.colorRegisterColors_ = colorRegisterColors;
    }

    public void SetKonstColors(IList<Color> konstColors) {
      this.konstColorImpls_ = konstColors;
    }

    private GxKonstColorSel tevStageColorConstantSel_;
    private GxKonstAlphaSel tevStageAlphaConstantSel_;

    public void UpdateKonst(GxKonstColorSel tevStageColorConstantSel,
                            GxKonstAlphaSel tevStageAlphaConstantSel) {
      this.tevStageColorConstantSel_ = tevStageColorConstantSel;
      this.tevStageAlphaConstantSel_ = tevStageAlphaConstantSel;
    }

    public bool TryGetEnumIndex_<T>(T value, T min, T max, out int index)
        where T : IComparable, IConvertible {
      var minCompare = value.CompareTo(min);
      var maxCompare = value.CompareTo(max);

      if (minCompare >= 0 && maxCompare <= 0) {
        index = value.ToInt32(null) - min.ToInt32(null);
        return true;
      }

      index = -1;
      return false;
    }

    // https://github.com/magcius/bmdview/blob/master/tev.markdown#gx_settevkcolorsel
    public IColorValue GetKonstColor_(GxKonstColorSel sel) {
      if (TryGetEnumIndex_(sel,
                           GxKonstColorSel.KCSel_1,
                           GxKonstColorSel.KCSel_1_8,
                           out var fracIndex)) {
        var numerator = 8 - fracIndex;
        var intensity = numerator / 8f;
        return this.equations_.CreateColorConstant(intensity);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstColorSel.KCSel_K0,
                           GxKonstColorSel.KCSel_K3,
                           out var rgbIndex)) {
        var konstRgb = this.konstColorImpls_[rgbIndex];
        return this.equations_.CreateColorConstant(
            konstRgb.R / 255d,
            konstRgb.G / 255d,
            konstRgb.B / 255d);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstColorSel.KCSel_K0_R,
                           GxKonstColorSel.KCSel_K3_R,
                           out var rIndex)) {
        var konstR = this.konstColorImpls_[rIndex];
        return this.equations_.CreateColorConstant(
            konstR.R / 255d);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstColorSel.KCSel_K0_G,
                           GxKonstColorSel.KCSel_K3_G,
                           out var gIndex)) {
        var konstG = this.konstColorImpls_[gIndex];
        return this.equations_.CreateColorConstant(
            konstG.G / 255d);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstColorSel.KCSel_K0_B,
                           GxKonstColorSel.KCSel_K3_B,
                           out var bIndex)) {
        var konstB = this.konstColorImpls_[bIndex];
        return this.equations_.CreateColorConstant(
            konstB.B / 255d);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstColorSel.KCSel_K0_A,
                           GxKonstColorSel.KCSel_K3_A,
                           out var aIndex)) {
        var konstA = this.konstColorImpls_[aIndex];
        return this.equations_.CreateColorConstant(
            konstA.A / 255d);
      }

      throw new NotImplementedException();
    }

    public IScalarValue GetKonstAlpha_(GxKonstAlphaSel sel) {
      if (TryGetEnumIndex_(sel,
                           GxKonstAlphaSel.KASel_1,
                           GxKonstAlphaSel.KASel_1_8,
                           out var fracIndex)) {
        var numerator = 8 - fracIndex;
        var intensity = numerator / 8f;
        return this.equations_.CreateScalarConstant(intensity);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstAlphaSel.KASel_K0_R,
                           GxKonstAlphaSel.KASel_K3_R,
                           out var rIndex)) {
        var konstR = this.konstColorImpls_[rIndex];
        return this.equations_.CreateScalarConstant(
            konstR.R / 255d);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstAlphaSel.KASel_K0_G,
                           GxKonstAlphaSel.KASel_K3_G,
                           out var gIndex)) {
        var konstG = this.konstColorImpls_[gIndex];
        return this.equations_.CreateScalarConstant(
            konstG.G / 255d);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstAlphaSel.KASel_K0_B,
                           GxKonstAlphaSel.KASel_K3_B,
                           out var bIndex)) {
        var konstB = this.konstColorImpls_[bIndex];
        return this.equations_.CreateScalarConstant(
            konstB.B / 255d);
      }

      if (TryGetEnumIndex_(sel,
                           GxKonstAlphaSel.KASel_K0_A,
                           GxKonstAlphaSel.KASel_K3_A,
                           out var aIndex)) {
        var konstA = this.konstColorImpls_[aIndex];
        return this.equations_.CreateScalarConstant(
            konstA.A / 255d);
      }

      throw new NotImplementedException();
    }

    public IColorValue GetColor(GxCc colorSource) {
      if (this.colorValues_.TryGetValue(colorSource, out var colorValue)) {
        return colorValue;
      }

      if (colorSource == GxCc.GX_CC_TEXC) {
        return this.GetTextureColorChannel_();
      }

      if (colorSource == GxCc.GX_CC_TEXA) {
        return this.GetTextureAlphaChannel_();
      }

      if (colorSource == GxCc.GX_CC_RASC ||
          colorSource == GxCc.GX_CC_RASA) {
        return this.GetVertexColorChannel_(colorSource);
      }

      if (colorSource == GxCc.GX_CC_KONST) {
        return this.GetKonstColor_(this.tevStageColorConstantSel_);
      }

      if (colorSource >= GxCc.GX_CC_C0 &&
          colorSource <= GxCc.GX_CC_A2) {
        var (colorRegister, isColor) =
            this.GetColorRegisterColorForSource_(colorSource);
        Asserts.Nonnull(colorRegister);
        var color = colorRegister.Color;
        var index = colorRegister.Index;

        if (isColor) {
          return this.registers_.GetOrCreateColorRegister(
              $"GxColor{index}",
              this.equations_.CreateColorConstant(
                  color.R / 255f,
                  color.G / 255f,
                  color.B / 255f));
        }

        return new ColorWrapper(
            this.registers_.GetOrCreateScalarRegister(
                $"GxAlpha{index}",
                this.equations_.CreateScalarConstant(
                    color.A / 255f)));
      }

      if (!GxFixedFunctionMaterial.STRICT) {
        return this.colorUndefined_;
      }

      throw new NotImplementedException();
    }

    public IScalarValue GetAlpha(GxCa alphaSource) {
      if (this.alphaValues_.TryGetValue(alphaSource, out var alphaValue)) {
        return alphaValue;
      }

      if (alphaSource == GxCa.GX_CA_TEXA) {
        return this.GetTextureAlphaChannelAsAlpha_();
      }

      if (alphaSource == GxCa.GX_CA_RASA) {
        return this.GetVertexAlphaChannel_();
      }

      if (alphaSource == GxCa.GX_CA_KONST) {
        return this.GetKonstAlpha_(this.tevStageAlphaConstantSel_);
      }

      if (TryGetEnumIndex_(
              alphaSource,
              GxCa.GX_CA_A0,
              GxCa.GX_CA_A2,
              out var caIndex)) {
        var colorRegister = this.GetColorRegisterColorForIndex_(caIndex);
        Asserts.Nonnull(colorRegister);
        var color = colorRegister.Color;
        var index = colorRegister.Index;

        return this.registers_.GetOrCreateScalarRegister($"GxAlpha{index}",
          this.equations_.CreateScalarConstant(
              color.A / 255f));
      }

      if (!GxFixedFunctionMaterial.STRICT) {
        return this.alphaUndefined_;
      }

      throw new NotImplementedException();
    }

    private (IColorRegister? colorRegister, bool isAlpha)
        GetColorRegisterColorForSource_(
            GxCc source) {
      var ccIndex = (int) source - (int) GxCc.GX_CC_C0;

      var isColor = ccIndex % 2 == 0;
      var index = isColor ? ccIndex / 2 : (ccIndex - 1) / 2;

      return (this.GetColorRegisterColorForIndex_(index), isColor);
    }

    private IColorRegister? GetColorRegisterColorForIndex_(int index) {
      if (this.colorRegisterColors_.Count > index) {
        return this.colorRegisterColors_[index];
      }

      return null;
    }
  }
}