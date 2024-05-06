using fin.image.io;

using grezzo.image;
using grezzo.schema.ctxb;

namespace grezzo.schema.cmb.tex {
  public class CtrTexture {
    private GlTextureFormat CollapseFormat_(GlTextureFormat format) {
      var lowerFormat = (GlTextureFormat) ((int) format & 0xFFFF);

      if (lowerFormat == GlTextureFormat.ETC1) {
        format = GlTextureFormat.ETC1;
      } else if (lowerFormat == GlTextureFormat.ETC1a4) {
        format = GlTextureFormat.ETC1a4;
      }

      return format;
    }

    public IImageReader DecodeImage(Texture texture)
      => new CmbImageReader(texture.width,
                            texture.height,
                            CollapseFormat_(texture.imageFormat));
  }
}