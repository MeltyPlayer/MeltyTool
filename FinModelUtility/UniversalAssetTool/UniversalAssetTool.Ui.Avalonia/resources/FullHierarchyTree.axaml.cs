using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;

using fin.model;
using fin.scene;
using fin.scene.components;
using fin.ui;
using fin.ui.rendering;
using fin.util.enumerables;
using fin.util.enums;

using Material.Icons;
using Material.Icons.Avalonia;

using uni.ui.avalonia.resources.scene;

namespace uni.ui.avalonia.resources;

[Flags]
public enum FullHierarchyTreeType {
  AREAS = 1 << 0,
  NODES = 1 << 1,
  MODELS = 1 << 2,
  BONES = 1 << 3,
  MESHES = 1 << 4,
  PRIMITIVES = 1 << 5,

  ALL = AREAS | NODES | MODELS | BONES | MESHES | PRIMITIVES,
}

public sealed class FullHierarchyTreeViewModelForDesigner
    : FullHierarchyTreeViewModel {
  public FullHierarchyTreeViewModelForDesigner() : base(
      SceneDesignerUtil.CreateStubScene()
                       .Areas.Select(a => new AreaFullHierarchyNode(a))
                       .ToArray()) { }
}

public class FullHierarchyTreeViewModel : BViewModel {
  public static FullHierarchyTreeViewModel FromScene(
      IReadOnlyScene scene,
      FullHierarchyTreeType type = FullHierarchyTreeType.ALL)
    => new(scene.Areas.Select(a => new AreaFullHierarchyNode(a, type))
                .ToArray());

  public static FullHierarchyTreeViewModel FromModel(
      IReadOnlyModel model,
      FullHierarchyTreeType type = FullHierarchyTreeType.ALL)
    => new(ModelFullHierarchyNode.GetChildren(model, type));

  public FullHierarchyTreeViewModel(IFullHierarchyNode[] rootNodes) {
    // TODO: Look this up from resource dictionary
    var regularFontSize = 11;

    this.Source
        = new HierarchicalTreeDataGridSource<IFullHierarchyNode>(rootNodes)
            .WithHierarchicalExpanderColumn(
                "Name",
                new TreeDataGridTemplateColumn {
                    Width = GridLength.Star,
                    CellTemplate = new FuncDataTemplate<IFullHierarchyNode>((_, _) => {
                      var stackPanel = new StackPanel {
                          Orientation = Orientation.Horizontal,
                      };
                      stackPanel.Children.AddRange([
                          new MaterialIcon {
                              Height = regularFontSize,
                              Margin = new Thickness(0),
                              [!MaterialIcon.KindProperty]
                                  = new Binding(nameof(IFullHierarchyNode.Icon)),
                          },
                          new TextBlock {
                              Classes = { "regular" },
                              Margin = new Thickness(3, 0, 0, 0),
                              VerticalAlignment = VerticalAlignment.Center,
                              [!TextBlock.TextProperty]
                                  = new Binding(nameof(IFullHierarchyNode.Name)),
                          }
                      ]);
                      return stackPanel;
                    })
                },
                x => x.Children,
                options: o => {
                  o.Width = GridLength.Star;
                });

    this.Source.ShowColumnHeaders = false;
    this.Source.CanUserResizeColumns = false;

    this.Source.RowSelection!.SelectionChanged += (_, e) => {
      IReadOnlyBone? selectedBone = null;
      IReadOnlyMesh? selectedMesh = null;
      IReadOnlySceneNode? selectedNode = null;

      if (e.SelectedItems.Count == 1) {
        var selectedFullHierarchyNode = e.SelectedItems[0];
        switch (selectedFullHierarchyNode) {
          case BoneFullHierarchyNode boneNode: {
            selectedBone = boneNode.Bone;
            break;
          }
          case MeshFullHierarchyNode meshNode: {
            selectedMesh = meshNode.Mesh;
            break;
          }
          case NodeFullHierarchyNode nodeNode: {
            selectedNode = nodeNode.Node;
            break;
          }
        }
      }

      SelectedBoneService.SelectBone(selectedBone);
      SelectedMeshService.SelectMesh(selectedMesh);
      SelectedNodeService.SelectNode(selectedNode);
    };
  }

  public HierarchicalTreeDataGridSource<IFullHierarchyNode> Source { get; }

  public void ExpandCollapse(FullHierarchyTreeType type)
    => this.Source.ExpandCollapseRecursive(n => type.CheckFlag(n.Type));
}

public interface IFullHierarchyNode {
  string Name { get; }
  MaterialIconKind Icon { get; }
  FullHierarchyTreeType Type { get; }
  IFullHierarchyNode[] Children { get; }
}

public sealed class AreaFullHierarchyNode(
    IReadOnlySceneArea area,
    IFullHierarchyNode[] children)
    : IFullHierarchyNode {
  public IReadOnlySceneArea Area => area;
  public string Name => "Area";
  public MaterialIconKind Icon => MaterialIconKind.ChartAreasplineVariant;
  public FullHierarchyTreeType Type => FullHierarchyTreeType.AREAS;
  public IFullHierarchyNode[] Children => children;

  public AreaFullHierarchyNode(
      IReadOnlySceneArea area,
      FullHierarchyTreeType type = FullHierarchyTreeType.ALL)
      : this(area, GetChildren(area, type)) { }

  public static IFullHierarchyNode[] GetChildren(
      IReadOnlySceneArea area,
      FullHierarchyTreeType type)
    => type.CheckFlag(FullHierarchyTreeType.NODES)
        ? ((area.CustomSkyboxNode != null
               ? new NodeFullHierarchyNode(area.CustomSkyboxNode, type)
               : null)
           .Yield()
           .Concat(
               area.RootNodes.Select(n => new NodeFullHierarchyNode(
                                         n,
                                         type)))
           .Nonnull()
           .ToArray())
        : [];
}

public sealed class NodeFullHierarchyNode(
    IReadOnlySceneNode node,
    IFullHierarchyNode[] children)
    : IFullHierarchyNode {
  public IReadOnlySceneNode Node => node;
  public string Name => node.Name ?? "Node";
  public MaterialIconKind Icon => MaterialIconKind.AccountOutline;
  public FullHierarchyTreeType Type => FullHierarchyTreeType.NODES;
  public IFullHierarchyNode[] Children => children;

  public NodeFullHierarchyNode(
      IReadOnlySceneNode node,
      FullHierarchyTreeType type = FullHierarchyTreeType.ALL)
      : this(node, GetChildren(node, type)) { }

  public static IFullHierarchyNode[] GetChildren(
      IReadOnlySceneNode node,
      FullHierarchyTreeType type)
    => (type.CheckFlag(FullHierarchyTreeType.MODELS)
           ? node.Components
                 .OfType<IModelRenderComponent>()
                 .Select(m => (IFullHierarchyNode) new ModelFullHierarchyNode(
                             m.Model,
                             type))
           : [])
       .Concat(
           (type.CheckFlag(FullHierarchyTreeType.NODES)
               ? node
                 .ChildNodes
                 .Select(n => new NodeFullHierarchyNode(n, type))
               : []))
       .ToArray();
}

public sealed class ModelFullHierarchyNode(
    IReadOnlyModel model,
    IFullHierarchyNode[] children)
    : IFullHierarchyNode {
  public IReadOnlyModel Model => model;
  public string Name => "Model";
  public MaterialIconKind Icon => MaterialIconKind.CubeOutline;
  public FullHierarchyTreeType Type => FullHierarchyTreeType.MODELS;
  public IFullHierarchyNode[] Children => children;

  public ModelFullHierarchyNode(
      IReadOnlyModel model,
      FullHierarchyTreeType type = FullHierarchyTreeType.ALL)
      : this(model, GetChildren(model, type)) { }

  public static IFullHierarchyNode[] GetChildren(
      IReadOnlyModel model,
      FullHierarchyTreeType type)
    => (type.CheckFlag(FullHierarchyTreeType.BONES)
           ? model
             .Skeleton
             .Root
             .Children
             .Select(b => (IFullHierarchyNode) new BoneFullHierarchyNode(
                         b,
                         type))
           : [])
       .Concat(
           (type.CheckFlag(FullHierarchyTreeType.MESHES)
               ? model
                 .Skin
                 .RootMeshes
                 .Select(m => new MeshFullHierarchyNode(m, type))
               : []))
       .ToArray();
}

public sealed class BoneFullHierarchyNode(
    IReadOnlyBone bone,
    IFullHierarchyNode[] children)
    : IFullHierarchyNode {
  public IReadOnlyBone Bone => bone;
  public string Name => bone.Name ?? $"Bone {bone.Index}";
  public MaterialIconKind Icon => MaterialIconKind.CardsDiamondOutline;
  public FullHierarchyTreeType Type => FullHierarchyTreeType.BONES;
  public IFullHierarchyNode[] Children => children;

  public BoneFullHierarchyNode(
      IReadOnlyBone bone,
      FullHierarchyTreeType type = FullHierarchyTreeType.ALL)
      : this(bone, GetChildren(bone, type)) { }

  public static IFullHierarchyNode[] GetChildren(
      IReadOnlyBone bone,
      FullHierarchyTreeType type)
    => type.CheckFlag(FullHierarchyTreeType.BONES)
        ? bone.Children.Select(b => new BoneFullHierarchyNode(b)).ToArray()
        : [];
}

public sealed class MeshFullHierarchyNode(
    IReadOnlyMesh mesh,
    IFullHierarchyNode[] children)
    : IFullHierarchyNode {
  public IReadOnlyMesh Mesh => mesh;
  public string Name => mesh.Name ?? $"Mesh {mesh.Index}";
  public MaterialIconKind Icon => MaterialIconKind.ShapeOutline;
  public FullHierarchyTreeType Type => FullHierarchyTreeType.BONES;
  public IFullHierarchyNode[] Children => children;

  public MeshFullHierarchyNode(
      IReadOnlyMesh mesh,
      FullHierarchyTreeType type = FullHierarchyTreeType.ALL)
      : this(mesh, GetChildren(mesh, type)) { }

  public static IFullHierarchyNode[] GetChildren(
      IReadOnlyMesh mesh,
      FullHierarchyTreeType type)
    => (type.CheckFlag(FullHierarchyTreeType.PRIMITIVES)
           ? mesh.Primitives.Select(p => (IFullHierarchyNode)
                                        new PrimitiveFullHierarchyNode(p))
           : [])
       .Concat(type.CheckFlag(FullHierarchyTreeType.MESHES)
                   ? mesh.SubMeshes.Select(b => new MeshFullHierarchyNode(
                                               b,
                                               type))
                   : [])
       .ToArray();
}

public sealed class PrimitiveFullHierarchyNode(IReadOnlyPrimitive primitive)
    : IFullHierarchyNode {
  public IReadOnlyPrimitive Primitive => primitive;

  public string Name
    => $"Primitive {primitive.Index} [{primitive.Type}, {primitive.Vertices.Count} vertices]";

  public MaterialIconKind Icon
    => primitive.Type switch {
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

  public FullHierarchyTreeType Type => FullHierarchyTreeType.PRIMITIVES;
  public IFullHierarchyNode[] Children => [];
}

public partial class FullHierarchyTree : UserControl {
  public FullHierarchyTree() {
    this.InitializeComponent();
  }
}