using fin.model;

namespace gx;

public class GxFixedFunctionBlending {
  public void ApplyBlending(IFixedFunctionMaterial material,
                            GxBlendMode blendMode,
                            GxBlendFactor srcFactor,
                            GxBlendFactor dstFactor,
                            GxLogicOp logicOp) {
      // Shamelessly copied from:
      // https://github.com/magcius/noclip.website/blob/c5a6d0137128065068b5842ffa9dff04f03eefdb/src/gx/gx_render.ts#L405-L423
      switch (blendMode) {
        case GxBlendMode.NONE: {
          material.SetBlending(
              BlendEquation.ADD,
              BlendFactor.ONE,
              BlendFactor.ZERO,
              LogicOp.UNDEFINED);
          break;
        }
        case GxBlendMode.BLEND: {
          material.SetBlending(
              BlendEquation.ADD,
              this.ConvertGxBlendFactorToFin_(srcFactor),
              this.ConvertGxBlendFactorToFin_(dstFactor),
              LogicOp.UNDEFINED);
          break;
        }
        case GxBlendMode.LOGIC: {
          // TODO: Might not be correct?
          material.SetBlending(
              BlendEquation.NONE,
              this.ConvertGxBlendFactorToFin_(srcFactor),
              this.ConvertGxBlendFactorToFin_(dstFactor),
              this.ConvertGxLogicOpToFin_(logicOp));
          break;
        }
        case GxBlendMode.SUBTRACT: {
          material.SetBlending(
              BlendEquation.REVERSE_SUBTRACT,
              BlendFactor.ONE,
              BlendFactor.ONE,
              LogicOp.UNDEFINED);
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }

  private BlendFactor ConvertGxBlendFactorToFin_(
      GxBlendFactor gxBlendFactor)
    => gxBlendFactor switch {
        GxBlendFactor.ZERO      => BlendFactor.ZERO,
        GxBlendFactor.ONE       => BlendFactor.ONE,
        GxBlendFactor.SRC_COLOR => BlendFactor.SRC_COLOR,
        GxBlendFactor.ONE_MINUS_SRC_COLOR => BlendFactor
            .ONE_MINUS_SRC_COLOR,
        GxBlendFactor.SRC_ALPHA => BlendFactor.SRC_ALPHA,
        GxBlendFactor.ONE_MINUS_SRC_ALPHA => BlendFactor
            .ONE_MINUS_SRC_ALPHA,
        GxBlendFactor.DST_ALPHA => BlendFactor.DST_ALPHA,
        GxBlendFactor.ONE_MINUS_DST_ALPHA => BlendFactor
            .ONE_MINUS_DST_ALPHA,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gxBlendFactor),
            gxBlendFactor,
            null)
    };

  private LogicOp ConvertGxLogicOpToFin_(GxLogicOp gxLogicOp)
    => gxLogicOp switch {
        GxLogicOp.CLEAR         => LogicOp.CLEAR,
        GxLogicOp.AND           => LogicOp.AND,
        GxLogicOp.AND_REVERSE   => LogicOp.AND_REVERSE,
        GxLogicOp.COPY          => LogicOp.COPY,
        GxLogicOp.AND_INVERTED  => LogicOp.AND_INVERTED,
        GxLogicOp.NOOP          => LogicOp.NOOP,
        GxLogicOp.XOR           => LogicOp.XOR,
        GxLogicOp.OR            => LogicOp.OR,
        GxLogicOp.NOR           => LogicOp.NOR,
        GxLogicOp.EQUIV         => LogicOp.EQUIV,
        GxLogicOp.INVERT        => LogicOp.INVERT,
        GxLogicOp.OR_REVERSE    => LogicOp.OR_REVERSE,
        GxLogicOp.COPY_INVERTED => LogicOp.COPY_INVERTED,
        GxLogicOp.OR_INVERTED   => LogicOp.OR_INVERTED,
        GxLogicOp.NAND          => LogicOp.NAND,
        GxLogicOp.SET           => LogicOp.SET,
        _ => throw new ArgumentOutOfRangeException(
            nameof(gxLogicOp),
            gxLogicOp,
            null)
    };
}