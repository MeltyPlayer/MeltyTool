using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.ssbo;
using fin.ui.rendering.gl.ubo;

namespace fin.ui.rendering.gl;

public sealed class PrimitiveMetadataSsbo {
  private readonly IReadOnlyList<IReadOnlyPrimitive> primitives_;

  private IReadOnlySet<IReadOnlyMaterial>? selectedMaterials_;
  private IReadOnlyMesh? selectedMesh_;

  private readonly int bufferSize_;
  private readonly GlSsbo impl_;

  public PrimitiveMetadataSsbo(IReadOnlyList<IReadOnlyPrimitive> primitives) {
    this.primitives_ = primitives;

    this.bufferSize_ = 4 * primitives.Sum(GetSubPrimitiveCount_);
    this.impl_ = new(this.bufferSize_,
                     GlslConstants.UBO_PRIMITIVE_METADATA_BINDING_INDEX);

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

  ~PrimitiveMetadataSsbo() => this.ReleaseUnmanagedResources_();

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
      var isSelectedByMesh
          = ReferenceEquals(primitive.Mesh, this.selectedMesh_);
      for (var i = 0; i < GetSubPrimitiveCount_(primitive); ++i) {
        UboUtil.AppendBool(
            buffer,
            ref offset,
            isSelectedByMaterial || isSelectedByMesh);
      }
    }

    this.impl_.UpdateDataIfChanged(buffer);
  }

  public void Bind() => this.impl_.Bind();

  private static int GetSubPrimitiveCount_(IReadOnlyPrimitive primitive)
    => primitive.Type switch {
        PrimitiveType.TRIANGLE_STRIP or PrimitiveType.TRIANGLE_FAN
            => primitive.Vertices.Count - 2,
        PrimitiveType.LINE_STRIP => primitive.Vertices.Count - 1,
        PrimitiveType.POINTS     => primitive.Vertices.Count,
        PrimitiveType.LINES      => primitive.Vertices.Count / 2,
        PrimitiveType.TRIANGLES  => primitive.Vertices.Count / 3,
        PrimitiveType.QUADS      => primitive.Vertices.Count / 4,
        PrimitiveType.QUAD_STRIP => (primitive.Vertices.Count - 2) / 2,
        _                        => throw new ArgumentOutOfRangeException()
    };
}