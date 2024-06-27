using fin.model;

namespace fin.ui.rendering.gl.material;

public interface IGlMaterialShader : IDisposable {
  IReadOnlyMaterial Material { get; }

  bool UseLighting { get; set; }
  bool DisposeTextures { get; set; }

  void Use();
}