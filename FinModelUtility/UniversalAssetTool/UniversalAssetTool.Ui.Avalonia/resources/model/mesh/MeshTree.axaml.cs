using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.model;
using fin.ui.rendering;
using fin.util.asserts;

using Material.Icons;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;


namespace uni.ui.avalonia.resources.model.mesh {
  public class MeshTreeViewModelForDesigner : MeshTreeViewModel {
    public MeshTreeViewModelForDesigner() {
      this.Meshes = ModelDesignerUtil.CreateStubModel().Skin.Meshes;
    }
  }

  public class MeshTreeViewModel : ViewModelBase {
    public required IReadOnlyList<IReadOnlyMesh>? Meshes {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RootNode = value != null
            ? new MeshTreeNode(value.Select(m => new MeshTreeNode(m)).ToArray())
            : null;
      }
    }

    public MeshTreeNode? RootNode {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }
  }

  public class MeshTreeNode : ViewModelBase {
    public MeshTreeNode(IReadOnlyList<MeshTreeNode> children)
      => this.Children = children;

    public MeshTreeNode(IReadOnlyMesh mesh) {
      this.Mesh = mesh;
      this.Children
          = mesh.SubMeshes
                .Select(m => new MeshTreeNode(m))
                .Concat(mesh.Primitives.Select(p => new MeshTreeNode(p)))
                .ToArray();
    }

    public MeshTreeNode(IReadOnlyPrimitive primitive) {
      this.Primitive = primitive;
      this.PrimitiveIcon = primitive.Type switch {
          PrimitiveType.POINTS => MaterialIconKind.VectorPoint,
          PrimitiveType.LINES
              or PrimitiveType.LINE_STRIP => MaterialIconKind.VectorLine,
          PrimitiveType.TRIANGLES
              or PrimitiveType.TRIANGLE_STRIP
              or PrimitiveType.TRIANGLE_FAN => MaterialIconKind.VectorTriangle,
          PrimitiveType.QUADS or PrimitiveType.QUAD_STRIP
              => MaterialIconKind.Rectangle,
          _ => throw new ArgumentOutOfRangeException()
      };
    }

    public IReadOnlyMesh? Mesh { get; }

    public IReadOnlyPrimitive? Primitive { get; }
    public MaterialIconKind PrimitiveIcon { get; }

    public IReadOnlyList<MeshTreeNode> Children { get; }

    public bool IsExpanded {
      get;
      set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;
  }

  public partial class MeshTree : UserControl {
    public MeshTree() {
      this.InitializeComponent();
    }

    private void SelectingItemsControl_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e) {
      if (e.AddedItems.Count > 0) {
        var addedItem = e.AddedItems[0];
        if (addedItem is MeshTreeNode {Mesh: not null} meshTreeNode) {
          SelectedMeshService.SelectMesh(meshTreeNode.Mesh);
          return;
        }
      }

      SelectedMeshService.SelectMesh(null);
    }
  }
}