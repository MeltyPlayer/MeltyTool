using System.Drawing;

using fin.image;
using fin.ui.rendering.gl.texture;

namespace fin.ui.rendering.gl.material;

public static class GlMaterialConstants {
  public static IGlTexture NULL_WHITE_TEXTURE { get; private set; }
  public static IGlTexture NULL_GRAY_TEXTURE { get; private set;}
  public static IGlTexture NULL_BLACK_TEXTURE { get; private set;}

  public static void Initialize() {
    if (NULL_WHITE_TEXTURE != null) {
      return;
    }

    NULL_WHITE_TEXTURE =
        new GlTexture(FinImage.Create1x1FromColor(Color.White));
    NULL_GRAY_TEXTURE =
        new GlTexture(FinImage.Create1x1FromColor(Color.Gray));
    NULL_BLACK_TEXTURE =
        new GlTexture(FinImage.Create1x1FromColor(Color.Black));
  }
}