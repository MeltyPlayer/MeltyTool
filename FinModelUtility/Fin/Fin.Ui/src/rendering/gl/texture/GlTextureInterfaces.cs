using fin.data.disposables;

namespace fin.ui.rendering.gl.texture;

public interface IGlTexture : IFinDisposable {
  int Width { get; }
  int Height { get; }

  int Id { get; }

  void Bind(int textureIndex = 0);
}