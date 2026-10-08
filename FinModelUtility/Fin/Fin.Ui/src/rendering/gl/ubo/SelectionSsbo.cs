using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.ssbo;
using fin.ui.rendering.gl.ubo;

namespace fin.ui.rendering.gl;

public sealed class SelectionSsbo {
  private readonly IReadOnlyList<IReadOnlyPrimitive> primitives_;

  private IReadOnlySet<IReadOnlyMaterial>? selectedMaterials_;
  private IReadOnlyMesh? selectedMesh_;

  private readonly int bufferSize_;
  private readonly GlSsbo impl_;

  public SelectionSsbo(IReadOnlyList<IReadOnlyPrimitive> primitives) {
    this.primitives_ = primitives;
    this.bufferSize_ = 4 * primitives.Count;
    this.impl_ = new(this.bufferSize_,
                     GlslConstants.UBO_SELECTION_BINDING_INDEX);

    SelectedMaterialsService.OnMaterialsSelected
        += selectedMaterials => {
          this.selectedMaterials_ = selectedMaterials;
          this.UpdateData_();
        };

    SelectedMeshService.OnMeshSelected
        += selectedMesh => {
          this.selectedMesh_ = selectedMesh;
          this.UpdateData_();
        };
  }

  ~SelectionSsbo() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_.Dispose();

  private void UpdateData_() {
    var offset = 0;
    Span<byte> buffer = stackalloc byte[this.bufferSize_];

    foreach (var primitive in this.primitives_) {
      var isSelectedByMaterial =
          primitive.Material != null &&
          (this.selectedMaterials_?.Contains(primitive.Material) ?? false);
      var isSelectedByMesh = primitive.Mesh == this.selectedMesh_;
      UboUtil.AppendBool(
          buffer,
          ref offset,
          isSelectedByMaterial || isSelectedByMesh);
    }

    this.impl_.UpdateDataIfChanged(buffer);
  }

  public void Bind() => this.impl_.Bind();
}