using System.Runtime.CompilerServices;

using fin.image.util;
using fin.math;
using fin.model;
using fin.model.util;
using fin.scene;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;
using fin.util.enumerables;

namespace fin.ui.rendering.gl.scene;

public interface IRenderGraphParams {
  ISceneNodeInstance Node { get; }
  IReadOnlyGlMaterialShader? GlMaterialShader { get; }
  bool IsSelected { get; }
  bool IsTransparent { get; }
  int MinPrimitiveIndex { get; }
  uint InversePriority { get; }
  int VaoId { get; }
}

public class RenderGraphComponentRenderer : IRenderGraphParams {
  public required ISceneNodeInstance Node { get; init; }
  public IReadOnlyGlMaterialShader? GlMaterialShader => null;
  public required ISceneNodeRenderComponent RenderComponent { get; init; }
  public bool IsSelected => false;
  public bool IsTransparent => true;
  public int MinPrimitiveIndex => int.MaxValue;
  public uint InversePriority => uint.MaxValue;
  public int VaoId => 0;

  public override string ToString() => "(Component)";
}

public class RenderGraphMaterialRenderer : IRenderGraphParams {
  public required ISceneNodeInstance Node { get; init; }
  public required IReadOnlyGlMaterialShader GlMaterialShader { get; init; }
  public required IModelRenderer ModelRenderer { get; init; }
  public required IMaterialRenderer MaterialRenderer { get; init; }
  public bool IsSelected => this.MaterialRenderer.IsSelected;
  public required bool IsTransparent { get; init; }
  public required int MinPrimitiveIndex { get; init; }
  public required uint InversePriority { get; init; }
  public int VaoId => this.ModelRenderer.VaoId;

  public override string ToString() {
    var material = this.GlMaterialShader.Material;
    var shaderProgram = this.GlMaterialShader.ShaderProgram;

    return
        $"(Material: {material?.Name ?? "(null)"}, {shaderProgram}, inverse priority: {this.InversePriority}, transparent: {this.IsTransparent})";
  }
}

public class RenderGraphElement : IComparable<RenderGraphElement> {
  public RenderGraphElement(IRenderGraphParams prms) {
    this.Params = prms;
    this.InitSortKey_();
  }

  public IRenderGraphParams Params { get; }
  public ulong SortKey { get; private set; }

  public override string ToString()
    => $"{this.Params}, Sort key: {this.SortKey}";

  // Loosely based on: https://realtimecollisiondetection.net/blog/?p=86
  // Transparent bits
  private const int TRANSPARENT_INVERSE_PRIORITY_BIT_COUNT_ = 16;

  private const ulong TRANSPARENT_INVERSE_PRIORITY_MASK_
      = ((ulong) 1 << TRANSPARENT_INVERSE_PRIORITY_BIT_COUNT_) - 1;

  private const int TRANSPARENT_MIN_PRIMITIVE_INDEX_BIT_COUNT_ = 16;

  private const ulong TRANSPARENT_MIN_PRIMITIVE_INDEX_MASK_
      = ((ulong) 1 << TRANSPARENT_MIN_PRIMITIVE_INDEX_BIT_COUNT_) - 1;

  private const int TRANSPARENT_DEPTH_BIT_COUNT_ = 31;

  private const ulong TRANSPARENT_DEPTH_MAX_
      = ((ulong) 1 << TRANSPARENT_DEPTH_BIT_COUNT_) - 1;

  // Opaque bits
  private const int OPAQUE_PROGRAM_ID_BIT_COUNT_ = 16;

  private const ulong OPAQUE_PROGRAM_ID_MASK_
      = ((ulong) 1 << OPAQUE_PROGRAM_ID_BIT_COUNT_) - 1;

  private const int OPAQUE_VAO_ID_BIT_COUNT_ = 12;

  private const ulong OPAQUE_VAO_ID_MASK_
      = ((ulong) 1 << OPAQUE_VAO_ID_BIT_COUNT_) - 1;

  private const int OPAQUE_MATERIAL_INDEX_BIT_COUNT_ = 12;

  private const ulong OPAQUE_MATERIAL_INDEX_MASK_
      = ((ulong) 1 << OPAQUE_MATERIAL_INDEX_BIT_COUNT_) - 1;

  private const int OPAQUE_DEPTH_BIT_COUNT_ = 23;

  private const ulong OPAQUE_DEPTH_MAX_
      = ((ulong) 1 << OPAQUE_DEPTH_BIT_COUNT_) - 1;

  private void InitSortKey_() {
    ulong sortKey = 0;

    var prms = this.Params;
    var materialShader = prms.GlMaterialShader;
    var material = materialShader?.Material;

    var isTransparent = prms.IsTransparent;
    sortKey |= (isTransparent ? (ulong) 1 : 0) << 63;

    if (isTransparent) {
      var inversePriorityBits
          = prms.InversePriority & TRANSPARENT_INVERSE_PRIORITY_MASK_;
      sortKey
          |= inversePriorityBits <<
             (TRANSPARENT_DEPTH_BIT_COUNT_ +
              TRANSPARENT_MIN_PRIMITIVE_INDEX_BIT_COUNT_);

      var minPrimitiveIndexBits
          = (ulong) prms.MinPrimitiveIndex &
            TRANSPARENT_MIN_PRIMITIVE_INDEX_MASK_;
      sortKey |= minPrimitiveIndexBits << TRANSPARENT_DEPTH_BIT_COUNT_;
    } else {
      var programIdBits
          = (ulong) (materialShader?.ShaderProgram.ProgramId ?? 0) &
            OPAQUE_PROGRAM_ID_MASK_;
      sortKey
          |= programIdBits <<
             (OPAQUE_DEPTH_BIT_COUNT_ +
              OPAQUE_MATERIAL_INDEX_BIT_COUNT_ +
              OPAQUE_VAO_ID_BIT_COUNT_);

      var vaoIdBits = (ulong) (prms.VaoId) & OPAQUE_VAO_ID_MASK_;
      sortKey
          |= vaoIdBits <<
             (OPAQUE_DEPTH_BIT_COUNT_ +
              OPAQUE_MATERIAL_INDEX_BIT_COUNT_);

      var materialIndexBits
          = (ulong) (material?.Index ?? 0) & OPAQUE_MATERIAL_INDEX_MASK_;
      sortKey |= materialIndexBits << OPAQUE_DEPTH_BIT_COUNT_;
    }

    this.SortKey = sortKey;
  }

  public void UpdateSortKey(ICamera camera,
                            float nearPlane,
                            float farPlane,
                            float scale) {
    ulong sortKey = this.SortKey;

    var prms = this.Params;
    var transform = prms.Node.Transform;

    var isTransparent = prms.IsTransparent;
    sortKey |= (isTransparent ? (ulong) 1 : 0) << 63;

    var distance = (camera.Position - transform.LocalTranslation).Length() *
                   scale;
    var distance0To1
        = ((distance - nearPlane) / (farPlane - nearPlane))
        .Clamp(0, 1);

    if (isTransparent) {
      var distance1To0 = 1 - distance0To1;
      var depthBits = (ulong) (distance1To0 * TRANSPARENT_DEPTH_MAX_);

      sortKey = (sortKey & ~TRANSPARENT_DEPTH_MAX_) | depthBits;
    } else {
      var depthBits = (ulong) (distance0To1 * OPAQUE_DEPTH_MAX_);
      sortKey = (sortKey & ~OPAQUE_DEPTH_MAX_) | depthBits;
    }

    this.SortKey = sortKey;
  }

  public int CompareTo(RenderGraphElement other)
    => this.SortKey.CompareTo(other.SortKey);

  public class Comparer : IComparer<RenderGraphElement> {
    public int Compare(RenderGraphElement? x, RenderGraphElement? y)
      => x.SortKey.CompareTo(y.SortKey);
  }
}

public sealed class SceneStaticRenderGraph : IRenderable {
  private readonly ISceneInstance scene_;

  private readonly
      List<(IReadOnlySceneNodeInstance, SimpleModelRenderComponent)>
      modelRenderComponents_ = new();

  private RenderGraphElement[] opaqueElements_;
  private RenderGraphElement[] transparentElements_;

  private ISkeletonRenderer[] skeletonRenderers_;

  private IReadOnlyBone? selectedBone_;
  private IReadOnlySceneNode? selectedNode_;
  private IReadOnlyMesh? selectedMesh_;
  private IReadOnlySet<IReadOnlyMaterial>? selectedMaterials_;

  public float Scale { get; set; }
  public float NearPlane { get; set; }
  public float FarPlane { get; set; }

  public SceneStaticRenderGraph(ISceneInstance scene) {
    this.scene_ = scene;

    SelectedBoneService.OnBoneSelected += selectedBone
        => this.selectedBone_ = selectedBone;
    SelectedNodeService.OnNodeSelected += selectedNode
        => this.selectedNode_ = selectedNode;
    SelectedMeshService.OnMeshSelected += selectedMesh
        => this.selectedMesh_ = selectedMesh;
    SelectedMaterialsService.OnMaterialsSelected += selectedMaterials
        => this.selectedMaterials_ = selectedMaterials;
  }

  ~SceneStaticRenderGraph() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() { }

  private void GenerateModelIfNull_() {
    if (this.opaqueElements_ != null) {
      return;
    }

    var elements = new LinkedList<RenderGraphElement>();
    foreach (var node in this.scene_.EnumerateAllNodes()) {
      foreach (var renderComponent in node.Definition.Components
                                          .OfType<
                                              ISceneNodeRenderComponent>()) {
        if (renderComponent is not SimpleModelRenderComponent
            modelRenderComponent) {
          elements.AddLast(
              new RenderGraphElement(
                  new RenderGraphComponentRenderer {
                      Node = node,
                      RenderComponent = renderComponent,
                  }));
          continue;
        }

        this.modelRenderComponents_.Add((node, modelRenderComponent));
        var modelRenderer = modelRenderComponent.ModelRenderer;
        modelRenderer.GenerateModelIfNull();

        foreach (var primitiveRenderer in modelRenderer.MaterialRenderers) {
          var isTransparent = primitiveRenderer.IsTransparent;
          elements.AddLast(
              new RenderGraphElement(
                  new RenderGraphMaterialRenderer {
                      Node = node,
                      GlMaterialShader = primitiveRenderer.GlMaterialShader,
                      IsTransparent = isTransparent,
                      MinPrimitiveIndex = primitiveRenderer.MinPrimitiveIndex,
                      InversePriority = primitiveRenderer.InversePriority,
                      ModelRenderer = modelRenderer,
                      MaterialRenderer = primitiveRenderer,
                  }));
        }
      }
    }

    this.opaqueElements_
        = elements.Where(e => !e.Params.IsTransparent).ToArray();

    var camera = Camera.Instance;
    foreach (var element in this.opaqueElements_) {
      element.UpdateSortKey(camera, this.NearPlane, this.FarPlane, this.Scale);
    }
    Array.Sort(this.opaqueElements_, comparer_);

    this.transparentElements_
        = elements.Where(e => e.Params.IsTransparent).ToArray();

    this.skeletonRenderers_
        = this.modelRenderComponents_
              .Select(t => t.Item2.SkeletonRenderer)
              .Nonnull()
              .ToArray();

    var uniqueModels = this.modelRenderComponents_
                           .Select(m => m.Item2.Model)
                           .Distinct()
                           .ToArray();
    var uniqueMaterials = uniqueModels
                          .SelectMany(m => m.MaterialManager.All)
                          .Distinct()
                          .ToArray();
    var uniqueTextures = uniqueMaterials
                         .SelectMany(m => m.Textures)
                         .Distinct()
                         .ToArray();

    DebugService.RenderGraphElementCount = elements.Count;
    DebugService.ModelCount = uniqueModels.Length;
    DebugService.MaterialCount = uniqueMaterials.Length;
    DebugService.OpaqueMaterialCount = uniqueMaterials.Count(m => m.GetTransparencyType() is TransparencyType.OPAQUE);
    DebugService.TransparentMaterialCount = DebugService.MaterialCount -
                                            DebugService.OpaqueMaterialCount;
    DebugService.FinTextureCount = uniqueTextures.Length;
  }

  public void Render() {
    this.GenerateModelIfNull_();

    foreach (var (node, modelRenderComponent) in this.modelRenderComponents_) {
      GlTransform.PushMatrix();
      GlTransform.MultMatrix(node.Transform.WorldMatrix);

      modelRenderComponent.TickAnimatables();
      modelRenderComponent.ModelRenderer.UpdateMatricesUbo();

      GlTransform.PopMatrix();
    }

    var camera = Camera.Instance;
    foreach (var element in this.transparentElements_) {
      element.UpdateSortKey(camera, this.NearPlane, this.FarPlane, this.Scale);
    }
    this.SortTransparentElements_();

    var isSomethingSelected = this.selectedNode_ != null ||
                              this.selectedMesh_ != null ||
                              (this.selectedMaterials_?.Any() ?? false);

    /*if (isSomethingSelected) {
      foreach (var element in this.elements_) {
        var prms = element.Params;
        if (prms.IsSelected || this.selectedNode_ == prms.Node.Definition) {
          GlUtil.RenderOutline(() => RenderParams_(prms));
        }
      }
    }*/

    foreach (var element in this.opaqueElements_) {
      var prms = element.Params;
      RenderParams_(prms);
    }

    if (isSomethingSelected) {
      foreach (var element in this.opaqueElements_) {
        var prms = element.Params;
        if (prms.IsSelected || this.selectedNode_ == prms.Node.Definition) {
          GlUtil.RenderHighlight(() => RenderParams_(prms));
        }
      }
    }

    foreach (var element in this.transparentElements_) {
      var prms = element.Params;
      RenderParams_(prms);
    }

    if (isSomethingSelected) {
      foreach (var element in this.transparentElements_) {
        var prms = element.Params;
        if (prms.IsSelected || this.selectedNode_ == prms.Node.Definition) {
          GlUtil.RenderHighlight(() => RenderParams_(prms));
        }
      }
    }

    if (this.selectedBone_ != null) {
      foreach (var skeletonRenderer in this.skeletonRenderers_) {
        skeletonRenderer.Render();
      }
    }
  }

  private static readonly RenderGraphElement.Comparer comparer_ = new();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SortTransparentElements_() {
    var elements = this.transparentElements_.AsSpan();
    if (AreSorted_(elements)) {
      return;
    }

    Array.Sort(this.transparentElements_, comparer_);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool AreSorted_(Span<RenderGraphElement> elements) {
    for (var i = 1; i < elements.Length; ++i) {
      if (comparer_.Compare(elements[i - 1], elements[i]) > 0) {
        return false;
      }
    }

    return true;
  }

  private static void RenderParams_(IRenderGraphParams prms) {
    GlTransform.PushMatrix();
    GlTransform.MultMatrix(prms.Node.Transform.WorldMatrix);

    switch (prms) {
      case RenderGraphComponentRenderer componentRenderer: {
        componentRenderer.RenderComponent.Render(prms.Node);
        break;
      }
      case RenderGraphMaterialRenderer primitiveRenderer: {
        primitiveRenderer.ModelRenderer.BindMatricesUbo();
        primitiveRenderer.MaterialRenderer.Render();
        break;
      }
    }

    GlTransform.PopMatrix();
  }
}