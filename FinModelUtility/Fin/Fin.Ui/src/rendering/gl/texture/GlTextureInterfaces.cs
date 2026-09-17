using fin.data.disposables;

namespace fin.ui.rendering.gl.texture;

public interface IGlSampler : IFinDisposable {
  int Id { get; }

  void Bind(int samplerIndex = 0);
}

public interface IGlTexture : IFinDisposable {
  int Id { get; }

  void Bind(int textureIndex = 0);
}