using fin.model;

namespace fin.ui.rendering.gl.model {
  public interface IModelRenderer : IRenderable, IDisposable {
    IReadOnlyModel Model { get; }
    ISet<IReadOnlyMesh> HiddenMeshes { get; }

    bool UseLighting { get; set; }
  }
}