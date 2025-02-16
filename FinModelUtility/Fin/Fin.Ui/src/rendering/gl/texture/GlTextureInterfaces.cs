using fin.data.disposables;

namespace fin.ui.rendering.gl.texture;

public interface IGlTexture : IFinDisposable {
  void Bind(int textureIndex = 0);
}