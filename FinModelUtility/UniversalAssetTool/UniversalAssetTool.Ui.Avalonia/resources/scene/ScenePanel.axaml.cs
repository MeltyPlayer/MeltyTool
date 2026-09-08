using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.data.dictionaries;
using fin.model;
using fin.scene;
using fin.ui;
using fin.ui.rendering;

using ReactiveUI;

using uni.ui.avalonia.resources.model.materials;
using uni.ui.avalonia.resources.scene.areas;

namespace uni.ui.avalonia.resources.scene;

public sealed class ScenePanelViewModelForDesigner : ScenePanelViewModel {
  public ScenePanelViewModelForDesigner() {
    this.Scene = SceneDesignerUtil.CreateStubScene();
  }
}

public class ScenePanelViewModel : BViewModel {
  public IReadOnlyScene Scene {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.AreasPanel = new AreasPanelViewModel { Scene = value, };
      this.FilesPanel = new FilesPanelViewModel(value);

      this.MaterialsPanel = new MaterialsPanelViewModel {
          ModelsAndMaterials
              = value.EnumerateAllDistinctModels()
                     .ToListDictionary(
                         m => m,
                         m => (IEnumerable<IReadOnlyMaterial?>) m
                             .MaterialManager.All),
      };
    }
  }

  public AreasPanelViewModel AreasPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public MaterialsPanelViewModel MaterialsPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public FilesPanelViewModel FilesPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class ScenePanel : UserControl {
  public ScenePanel() {
    this.InitializeComponent();
  }

  private void ClearSelectedItemsWhenTabChanged_(
      object? sender,
      SelectionChangedEventArgs e) {
    if (e.Source != this.ModelTabs) {
      return;
    }

    var shouldDeselectArea = true;
    if (e.AddedItems.Count > 0) {
      if (e.AddedItems[0] is TabItem item) {
        var header = item.Header;

        if (header == this.AreasTabHeader) {
          shouldDeselectArea = true;
        }
      }
    }

    if (shouldDeselectArea) {
      SelectedNodeService.SelectNode(null);
    }
  }
}