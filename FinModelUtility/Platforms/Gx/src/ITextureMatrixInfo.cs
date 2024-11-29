using System.Numerics;

namespace gx;

public interface ITextureMatrixInfo {
  GxTexGenType TexGenType { get; }
  Vector3 Center { get; }
  Vector2 Scale { get; }
  Vector2 Translation { get; }
  short Rotation { get; }
}

public record TextureMatrixInfoImpl(
    GxTexGenType TexGenType,
    Vector3 Center,
    Vector2 Scale,
    Vector2 Translation,
    short Rotation) : ITextureMatrixInfo;