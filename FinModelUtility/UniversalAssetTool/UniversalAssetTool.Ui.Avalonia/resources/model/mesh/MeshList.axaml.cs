using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.model;
using fin.ui.rendering;
using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.model.mesh {
  public class MeshListViewModelForDesigner
      : MeshListViewModel {
    public MeshListViewModelForDesigner() {
      this.Meshes = ModelDesignerUtil.CreateStubModel().Skin.Meshes;
    }
  }

  public record MeshWithName(string Name, IReadOnlyMesh Mesh);

  public class MeshListViewModel : ViewModelBase {
    private IReadOnlyList<IReadOnlyMesh>? meshes_;
    private IReadOnlyList<MeshWithName>? meshesWithNames_;

    public required IReadOnlyList<IReadOnlyMesh>? Meshes {
      get => this.meshes_;
      set {
        this.RaiseAndSetIfChanged(ref this.meshes_, value);
        this.MeshesWithNames
            = value
              ?.Select(
                  (mesh, i)
                      => new MeshWithName(mesh.Name ?? $"(Mesh {i})", mesh))
              .ToArray();
      }
    }

    public IReadOnlyList<MeshWithName>? MeshesWithNames {
      get => this.meshesWithNames_;
      private set
        => this.RaiseAndSetIfChanged(ref this.meshesWithNames_, value);
    }
  }

  public partial class MeshList : UserControl {
    public MeshList() {
      this.InitializeComponent();
    }

    protected void SelectingItemsControl_OnSelectionChanged(
        object? sender,
        SelectionChangedEventArgs e) {
      if (e.AddedItems.Count == 0) {
        SelectedMeshService.SelectMesh(null);
        return;
      }

      var selectedMesh = Asserts.AsA<MeshWithName>(e.AddedItems[0]);
      SelectedMeshService.SelectMesh(selectedMesh.Mesh);
    }
  }
}