using fin.schema.vector;

namespace gx;

public interface ITextureMatrixInfo {
  GxTexGenType TexGenType { get; }
  Vector2f Scale { get; }
  Vector2f Translation { get; }
  short Rotation { get; }
}

public class TextureMatrixInfoImpl : ITextureMatrixInfo {
  public GxTexGenType TexGenType { get; set; }
  public Vector2f Scale { get; set; }
  public Vector2f Translation { get; set; }
  public short Rotation { get; set; }
}