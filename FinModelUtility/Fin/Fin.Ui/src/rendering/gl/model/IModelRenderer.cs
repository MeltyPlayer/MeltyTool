using fin.model;
using fin.ui.rendering.gl.material;

namespace fin.ui.rendering.gl.model;

public interface IModelRenderer : IRenderable, IDisposable {
  IReadOnlyModel Model { get; }
  IReadOnlySet<IReadOnlyMesh>? HiddenMeshes { get; set; }

  void GenerateModelIfNull();
  IEnumerable<IGlMaterialShader> GetMaterialShaders(IReadOnlyMaterial material);
}

public interface IDynamicModelRenderer : IModelRenderer {
  void UpdateBuffer();
}