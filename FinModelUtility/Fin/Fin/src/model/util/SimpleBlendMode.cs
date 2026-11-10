namespace fin.model.util;

public enum SimpleBlendMode {
  NONE,
  NORMAL,
  ADD,
  DIFFERENCE,
  MULTIPLY,
  SCREEN,
  DARKEN,
  LIGHTEN,
}

public static class SimpleBlendModeExtensions {
  extension(IMaterial material) {
    public void SetSimpleBlending(SimpleBlendMode simpleBlendMode) {
      switch (simpleBlendMode) {
        case SimpleBlendMode.NONE: {
          material.SetBlending(BlendEquation.ADD,
                               BlendFactor.ONE,
                               BlendFactor.ZERO,
                               LogicOp.UNDEFINED);
          break;
        }
        case SimpleBlendMode.NORMAL: {
          material.SetBlending(
              BlendEquation.ADD,
              BlendFactor.SRC_ALPHA,
              BlendFactor.ONE_MINUS_SRC_ALPHA,
              LogicOp.UNDEFINED);
          break;
        }
        case SimpleBlendMode.ADD: {
          material.SetBlending(
              BlendEquation.ADD,
              BlendFactor.ONE,
              BlendFactor.ONE,
              LogicOp.UNDEFINED);
          break;
        }
        case SimpleBlendMode.DIFFERENCE: {
          material.SetBlending(
              BlendEquation.REVERSE_SUBTRACT,
              BlendFactor.ONE,
              BlendFactor.ONE,
              LogicOp.UNDEFINED);
          break;
        }
        case SimpleBlendMode.MULTIPLY: {
          material.SetBlending(
              BlendEquation.ADD,
              BlendFactor.DST_COLOR,
              BlendFactor.ZERO,
              LogicOp.UNDEFINED);
          break;
        }
        case SimpleBlendMode.SCREEN: {
          material.SetBlending(
              BlendEquation.ADD,
              BlendFactor.ONE,
              BlendFactor.ONE_MINUS_SRC_COLOR,
              LogicOp.UNDEFINED);
          break;
        }
        case SimpleBlendMode.DARKEN: {
          material.SetBlending(
              BlendEquation.MIN,
              BlendFactor.ONE,
              BlendFactor.ONE,
              LogicOp.UNDEFINED);
          break;
        }
        case SimpleBlendMode.LIGHTEN: {
          material.SetBlending(
              BlendEquation.MAX,
              BlendFactor.ONE,
              BlendFactor.ONE,
              LogicOp.UNDEFINED);
          break;
        }
      }
    }
  }
}