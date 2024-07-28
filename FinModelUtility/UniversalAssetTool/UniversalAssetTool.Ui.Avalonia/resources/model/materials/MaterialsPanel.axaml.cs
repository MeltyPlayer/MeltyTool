using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Data.Converters;

using fin.model;
using fin.ui.rendering;

using NaturalSort.Extension;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.model.materials {
  public class MaterialsPanelViewModelForDesigner
      : MaterialsPanelViewModel {
    public MaterialsPanelViewModelForDesigner() {
      var (model, material) = ModelDesignerUtil.CreateStubModelAndMaterial();
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
                materials.OrderBy(m => m?.Name,
                                  new NaturalSortComparer(
                                      StringComparison.OrdinalIgnoreCase))
                         .Select((m, i) => (i, m)));
      }
    }

    public ObservableCollection<(int, IReadOnlyMaterial?)> Materials {
      get => this.materials_;
      private set {
        this.RaiseAndSetIfChanged(ref this.materials_, value);
        this.SelectedMaterial = this.Materials.FirstOrDefault();
      }
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
        SelectedMaterialsService.SelectMaterial(this.selectedMaterial_?.Item2);
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
      this.InitializeComponent();
    }

    public static readonly IValueConverter GetMaterialLabel =
        new FuncValueConverter<(int, IReadOnlyMaterial?), string>(
            x => {
              var (i, m) = x;
              return $"Material {i}: {(m?.Name ?? "(null)")}";
            });
  }
}