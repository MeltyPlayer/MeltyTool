namespace gx;

public interface IIndirectTexture {
  // TODO: Inaccurate, more like extra TEV stages
  GxTexCoord TexCoord { get; }
  GxTexMap TexMap { get; }
}