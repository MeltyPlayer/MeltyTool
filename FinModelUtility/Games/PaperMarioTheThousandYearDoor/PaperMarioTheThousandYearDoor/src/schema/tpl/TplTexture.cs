using fin.image;

namespace ttyd.schema.tpl;

public class TplTexture {
  public required TplTextureHeader Header { get; init; }
  public required TplTexturePalette? Palette { get; init; }
  public required IImage Image { get; init; }
}