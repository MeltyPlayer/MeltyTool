using System.Numerics;

using fin.model;

namespace sm64ds.schema.bmd;

public class TextureParams {
  public Vector2 Translation { get; set; }
  public float Rotation { get; set; }
  public Vector2 Scale { get; set; }

  public WrapMode WrapModeS { get; set; }
  public WrapMode WrapModeT { get; set; }
}

public static class TextureParamsUtil {
  public static TextureParams GetParams(Material material,
                                            Texture texture) {
    var mergedTextureParamsValue
        = material.TextureParameters | texture.Parameters;

    var textureParams = new TextureParams();

    var textureCoordTransformMode = mergedTextureParamsValue >> 30;
    switch (textureCoordTransformMode) {
      case 0: {
        textureParams.Translation = Vector2.Zero;
        textureParams.Rotation = 0;
        textureParams.Scale = Vector2.One;
        break;
      }
      case 1 or 2 or 3: {
        textureParams.Translation = (Vector2) material.TextureTranslation;
        textureParams.Rotation
            = material.TextureRotation * (float) Math.PI / 2048.0f;
        textureParams.Scale = (Vector2) material.TextureScale;
        break;
      }
    }

    textureParams.WrapModeS = (mergedTextureParamsValue & 0x10000) == 0x10000
        ? (mergedTextureParamsValue & 0x40000) == 0x40000
            ? WrapMode.MIRROR_CLAMP
            : WrapMode.REPEAT
        : WrapMode.CLAMP;
    textureParams.WrapModeT = (mergedTextureParamsValue & 0x20000) == 0x20000
        ? (mergedTextureParamsValue & 0x80000) == 0x80000
            ? WrapMode.MIRROR_CLAMP
            : WrapMode.REPEAT
        : WrapMode.CLAMP;

    return textureParams;
  }
}