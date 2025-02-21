using System.Numerics;

using fin.math;
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

    textureParams.WrapModeS = GetWrapMode_(mergedTextureParamsValue, 16);
    textureParams.WrapModeT = GetWrapMode_(mergedTextureParamsValue, 17);

    return textureParams;
  }

  private static WrapMode GetWrapMode_(uint prms, int offset) {
    var repeat = prms.GetBit(offset);
    var mirror = prms.GetBit(offset + 2);
    return (mirror, repeat) switch {
        (true, true)   => WrapMode.MIRROR_REPEAT,
        (true, false)  => WrapMode.MIRROR_CLAMP,
        (false, true)  => WrapMode.REPEAT,
        (false, false) => WrapMode.CLAMP,
    };
  }
}