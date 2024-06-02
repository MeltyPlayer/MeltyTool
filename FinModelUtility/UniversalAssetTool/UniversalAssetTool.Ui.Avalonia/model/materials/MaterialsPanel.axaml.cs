using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Data.Converters;

using fin.model;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.model.materials {
  public class MaterialsPanelViewModelForDesigner
      : MaterialsPanelViewModel {
    public MaterialsPanelViewModelForDesigner() {
      var (model, material) = MaterialDesignerUtil.CreateStubModelAndMaterial();
      this.ModelAndMaterials = (model, new[] { material, material, material });
    }
  }

  public class MaterialsPanelViewModel : ViewModelBase {
    private (IReadOnlyModel, IReadOnlyList<IReadOnlyMaterial?>)
        modelAndMaterials_;

    private ObservableCollection<(int, IReadOnlyMaterial?)> materials_;
    private (int, IReadOnlyMaterial?)? selectedMaterial_;
    private MaterialPanelViewModel? selectedMaterialPanelViewModel_;

    public (IReadOnlyModel, IReadOnlyList<IReadOnlyMaterial?>)
        ModelAndMaterials {
      get => this.modelAndMaterials_;
      set {
        this.RaiseAndSetIfChanged(ref this.modelAndMaterials_, value);

        var (_, materials) = value;
        this.Materials
            = new ObservableCollection<(int, IReadOnlyMaterial?)>(
                materials.Select((m, i) => (i, m)));

        this.SelectedMaterial = this.Materials.FirstOrDefault();
      }
    }

    public ObservableCollection<(int, IReadOnlyMaterial?)> Materials {
      get => this.materials_;
      private set => this.RaiseAndSetIfChanged(ref this.materials_, value);
    }

    public (int, IReadOnlyMaterial?)? SelectedMaterial {
      get => this.selectedMaterial_;
      set {
        this.RaiseAndSetIfChanged(ref this.selectedMaterial_, value);
        this.SelectedMaterialPanel
            = value != null
                ? new MaterialPanelViewModel {
                    ModelAndMaterial = (
                        this.modelAndMaterials_.Item1, value.Value.Item2),
                }
                : null;
      }
    }

    public MaterialPanelViewModel? SelectedMaterialPanel {
      get => this.selectedMaterialPanelViewModel_;
      private set => this.RaiseAndSetIfChanged(
          ref this.selectedMaterialPanelViewModel_,
          value);
    }
  }

  public partial class MaterialsPanel : UserControl {
    public MaterialsPanel() {
      InitializeComponent();
    }

    public static readonly IValueConverter GetMaterialLabel =
        new FuncValueConverter<(int, IReadOnlyMaterial?), string>(
            x => {
              var (i, m) = x;
              return $"Material {i}: {(m?.Name ?? "(null)")}";
            });
  }
}