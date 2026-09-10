using System.Drawing;

using OpenTK.Graphics.OpenGL4;

using FinLogicOp = fin.model.LogicOp;
using FinBlendEquation = fin.model.BlendEquation;
using GlBlendEquation = OpenTK.Graphics.OpenGL4.BlendEquationMode;
using FinBlendFactor = fin.model.BlendFactor;
using GlBlendFactorSrc = OpenTK.Graphics.OpenGL4.BlendingFactorSrc;
using GlBlendFactorDst = OpenTK.Graphics.OpenGL4.BlendingFactorDest;


namespace fin.ui.rendering.gl;

public partial record GlState {
  public Color BlendColor { get; set; }

  public (FinBlendEquation colorBlendEquation, FinBlendFactor colorSrcFactor,
      FinBlendFactor colorDstFactor,
      FinBlendEquation alphaBlendEquation, FinBlendFactor alphaSrcFactor,
      FinBlendFactor alphaDstFactor, FinLogicOp logicOp)
      CurrentBlending { get; set; } = (
      FinBlendEquation.ADD, FinBlendFactor.SRC_ALPHA,
      FinBlendFactor.ONE_MINUS_SRC_ALPHA,
      FinBlendEquation.ADD, FinBlendFactor.SRC_ALPHA,
      FinBlendFactor.ONE_MINUS_SRC_ALPHA,
      FinLogicOp.UNDEFINED);
}

public static partial class GlUtil {
  public static bool DisableChangingBlending { get; set; }

  public static void SetBlendColor(in Color color) {
    if (currentState_.BlendColor == color) {
      return;
    }

    currentState_.BlendColor = color;
    GL.BlendColor(color);
  }

  public static Color GetBlendColor() => currentState_.BlendColor;

  public static void ResetBlending() {
    SetBlendColor(Color.White);
    SetBlending(
        FinBlendEquation.ADD,
        FinBlendFactor.SRC_ALPHA,
        FinBlendFactor.ONE_MINUS_SRC_ALPHA);
  }

  public static bool SetBlending(
      FinBlendEquation blendEquation,
      FinBlendFactor srcFactor,
      FinBlendFactor dstFactor,
      FinLogicOp logicOp = FinLogicOp.UNDEFINED)
    => SetBlendingSeparate(blendEquation,
                           srcFactor,
                           dstFactor,
                           FinBlendEquation.ADD,
                           FinBlendFactor.ONE,
                           FinBlendFactor.ONE,
                           logicOp);

  public static bool SetBlendingSeparate(
      FinBlendEquation colorBlendEquation,
      FinBlendFactor colorSrcFactor,
      FinBlendFactor colorDstFactor,
      FinBlendEquation alphaBlendEquation,
      FinBlendFactor alphaSrcFactor,
      FinBlendFactor alphaDstFactor,
      FinLogicOp logicOp = FinLogicOp.UNDEFINED) {
    if (DisableChangingBlending) {
      return false;
    }

    if (currentState_.CurrentBlending ==
        (colorBlendEquation, colorSrcFactor, colorDstFactor,
         alphaBlendEquation, alphaSrcFactor, alphaDstFactor, logicOp)) {
      return false;
    }

    currentState_.CurrentBlending =
        (colorBlendEquation, colorSrcFactor, colorDstFactor,
         alphaBlendEquation, alphaSrcFactor, alphaDstFactor, logicOp);

    var isColorNone = colorBlendEquation is FinBlendEquation.NONE;
    var isAlphaNone = alphaBlendEquation is FinBlendEquation.NONE;

    if (isColorNone && isAlphaNone) {
      GL.Disable(EnableCap.Blend);
      GL.BlendEquation(GlBlendEquation.FuncAdd);
      GL.BlendFunc(BlendingFactor.SrcAlpha,
                   BlendingFactor.OneMinusSrcAlpha);
    } else {
      GL.Enable(EnableCap.Blend);

      GlBlendEquation colorBlendEquationGl = GlBlendEquation.FuncAdd;
      GlBlendFactorSrc colorSrcFactorGl = GlBlendFactorSrc.SrcAlpha;
      GlBlendFactorDst colorDstFactorGl = GlBlendFactorDst.OneMinusSrcAlpha;
      if (!isColorNone) {
        colorBlendEquationGl =
            ConvertFinBlendEquationToGl_(colorBlendEquation);
        colorSrcFactorGl = ConvertFinBlendFactorToGlSrc_(colorSrcFactor);
        colorDstFactorGl = ConvertFinBlendFactorToGlDst_(colorDstFactor);
      }

      GlBlendEquation alphaBlendEquationGl = GlBlendEquation.FuncAdd;
      GlBlendFactorSrc alphaSrcFactorGl = GlBlendFactorSrc.SrcAlpha;
      GlBlendFactorDst alphaDstFactorGl = GlBlendFactorDst.OneMinusSrcAlpha;
      if (!isAlphaNone) {
        alphaBlendEquationGl =
            ConvertFinBlendEquationToGl_(alphaBlendEquation);
        alphaSrcFactorGl = ConvertFinBlendFactorToGlSrc_(alphaSrcFactor);
        alphaDstFactorGl = ConvertFinBlendFactorToGlDst_(alphaDstFactor);
      }

      GL.BlendEquationSeparate(colorBlendEquationGl, alphaBlendEquationGl);
      GL.BlendFuncSeparate(colorSrcFactorGl,
                           colorDstFactorGl,
                           alphaSrcFactorGl,
                           alphaDstFactorGl);
    }

    if (logicOp != FinLogicOp.UNDEFINED) {
      GL.Enable(EnableCap.ColorLogicOp);
      GL.LogicOp(logicOp switch {
          FinLogicOp.AND           => LogicOp.And,
          FinLogicOp.AND_INVERTED  => LogicOp.AndInverted,
          FinLogicOp.AND_REVERSE   => LogicOp.AndReverse,
          FinLogicOp.CLEAR         => LogicOp.Clear,
          FinLogicOp.COPY          => LogicOp.Copy,
          FinLogicOp.COPY_INVERTED => LogicOp.CopyInverted,
          FinLogicOp.EQUIV         => LogicOp.Equiv,
          FinLogicOp.INVERT        => LogicOp.Invert,
          FinLogicOp.NAND          => LogicOp.Nand,
          FinLogicOp.NOOP          => LogicOp.Noop,
          FinLogicOp.NOR           => LogicOp.Nor,
          FinLogicOp.OR            => LogicOp.Or,
          FinLogicOp.OR_INVERTED   => LogicOp.OrInverted,
          FinLogicOp.OR_REVERSE    => LogicOp.OrReverse,
          FinLogicOp.SET           => LogicOp.Set,
          FinLogicOp.XOR           => LogicOp.Xor,
          _                        => throw new ArgumentOutOfRangeException(nameof(logicOp), logicOp, null)
      });
    } else {
      GL.Disable(EnableCap.ColorLogicOp);
    }

    return true;
  }

  private static GlBlendEquation ConvertFinBlendEquationToGl_(
      FinBlendEquation finBlendEquation)
    => finBlendEquation switch {
        FinBlendEquation.ADD      => GlBlendEquation.FuncAdd,
        FinBlendEquation.SUBTRACT => GlBlendEquation.FuncSubtract,
        FinBlendEquation.REVERSE_SUBTRACT => GlBlendEquation
            .FuncReverseSubtract,
        FinBlendEquation.MIN => GlBlendEquation.Min,
        FinBlendEquation.MAX => GlBlendEquation.Max,
        _ => throw new ArgumentOutOfRangeException(
            nameof(finBlendEquation),
            finBlendEquation,
            null)
    };

  private static GlBlendFactorSrc ConvertFinBlendFactorToGlSrc_(
      FinBlendFactor finBlendFactor)
    => finBlendFactor switch {
        FinBlendFactor.ZERO      => GlBlendFactorSrc.Zero,
        FinBlendFactor.ONE       => GlBlendFactorSrc.One,
        FinBlendFactor.SRC_COLOR => GlBlendFactorSrc.SrcColor,
        FinBlendFactor.ONE_MINUS_SRC_COLOR => GlBlendFactorSrc
            .OneMinusSrcColor,
        FinBlendFactor.SRC_ALPHA => GlBlendFactorSrc.SrcAlpha,
        FinBlendFactor.ONE_MINUS_SRC_ALPHA => GlBlendFactorSrc
            .OneMinusSrcAlpha,
        FinBlendFactor.DST_COLOR => GlBlendFactorSrc.DstColor,
        FinBlendFactor.ONE_MINUS_DST_COLOR => GlBlendFactorSrc
            .OneMinusDstColor,
        FinBlendFactor.DST_ALPHA => GlBlendFactorSrc.DstAlpha,
        FinBlendFactor.ONE_MINUS_DST_ALPHA => GlBlendFactorSrc
            .OneMinusDstAlpha,
        FinBlendFactor.CONST_COLOR => GlBlendFactorSrc.ConstantColor,
        FinBlendFactor.ONE_MINUS_CONST_COLOR => GlBlendFactorSrc
            .OneMinusConstantColor,
        FinBlendFactor.CONST_ALPHA => GlBlendFactorSrc.ConstantAlpha,
        FinBlendFactor.ONE_MINUS_CONST_ALPHA => GlBlendFactorSrc
            .OneMinusConstantAlpha,
        _ => throw new ArgumentOutOfRangeException(
            nameof(finBlendFactor),
            finBlendFactor,
            null)
    };

  private static GlBlendFactorDst ConvertFinBlendFactorToGlDst_(
      FinBlendFactor finBlendFactor)
    => finBlendFactor switch {
        FinBlendFactor.ZERO      => GlBlendFactorDst.Zero,
        FinBlendFactor.ONE       => GlBlendFactorDst.One,
        FinBlendFactor.SRC_COLOR => GlBlendFactorDst.SrcColor,
        FinBlendFactor.ONE_MINUS_SRC_COLOR => GlBlendFactorDst
            .OneMinusSrcColor,
        FinBlendFactor.SRC_ALPHA => GlBlendFactorDst.SrcAlpha,
        FinBlendFactor.ONE_MINUS_SRC_ALPHA => GlBlendFactorDst
            .OneMinusSrcAlpha,
        FinBlendFactor.DST_COLOR => GlBlendFactorDst.DstColor,
        FinBlendFactor.ONE_MINUS_DST_COLOR => GlBlendFactorDst
            .OneMinusDstColor,
        FinBlendFactor.DST_ALPHA => GlBlendFactorDst.DstAlpha,
        FinBlendFactor.ONE_MINUS_DST_ALPHA => GlBlendFactorDst
            .OneMinusDstAlpha,
        FinBlendFactor.CONST_COLOR => GlBlendFactorDst.ConstantColor,
        FinBlendFactor.ONE_MINUS_CONST_COLOR => GlBlendFactorDst
            .OneMinusConstantColor,
        FinBlendFactor.CONST_ALPHA => GlBlendFactorDst.ConstantAlpha,
        FinBlendFactor.ONE_MINUS_CONST_ALPHA => GlBlendFactorDst
            .OneMinusConstantAlpha,
        _ => throw new ArgumentOutOfRangeException(
            nameof(finBlendFactor),
            finBlendFactor,
            null)
    };
}