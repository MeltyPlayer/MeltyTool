using fin.schema.vector;

namespace gx;

public interface ITextureMatrixInfo {
  GxTexGenType TexGenType { get; }
  Vector3f Center { get; }
  Vector2f Scale { get; }
  Vector2f Translation { get; }
  short Rotation { get; }
}

public record TextureMatrixInfoImpl(
    GxTexGenType TexGenType,
    Vector3f Center,
    Vector2f Scale,
    Vector2f Translation,
    short Rotation) : ITextureMatrixInfo;