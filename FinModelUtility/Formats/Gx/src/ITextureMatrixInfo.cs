using fin.schema.vector;

namespace gx;

public interface ITextureMatrixInfo {
  GxTexGenType TexGenType { get; }
  Vector2f Scale { get; }
  Vector2f Translation { get; }
  short Rotation { get; }
}