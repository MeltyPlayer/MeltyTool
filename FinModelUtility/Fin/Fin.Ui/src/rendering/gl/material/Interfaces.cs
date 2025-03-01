using fin.model;

namespace fin.ui.rendering.gl.material;

public interface IGlMaterialShader : IDisposable {
  IReadOnlyMaterial Material { get; }

  void Use();
}