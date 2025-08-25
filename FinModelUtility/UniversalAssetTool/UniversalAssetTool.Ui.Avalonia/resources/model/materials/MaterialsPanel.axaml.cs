using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Data.Converters;

using fin.model;
using fin.ui.avalonia;
using fin.ui.rendering;

using NaturalSort.Extension;

using ReactiveUI;

namespace uni.ui.avalonia.resources.model.materials {
  public class MaterialsPanelViewModelForDesigner
      : MaterialsPanelViewModel {
    public MaterialsPanelViewModelForDesigner() {
      var (model, material) = ModelDesignerUtil.CreateStubModelAndMaterial();
      this.ModelAndMaterials = (model, [material, material, material]);
    }
  }

  public class MaterialsPanelViewModel : ViewModelBase {
    private (IReadOnlyModel, IReadOnlyList<IReadOnlyMaterial?>)
        modelAndMaterials_;

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
      get;
      private set {
        this.RaiseAndSetIfChanged(ref field, value);
        this.SelectedMaterial = this.Materials.FirstOrDefault();
      }
    }

    public (int, IReadOnlyMaterial?)? SelectedMaterial {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        this.SelectedMaterialPanel
            = value != null
                ? new MaterialPanelViewModel {
                    ModelAndMaterial = (
                        this.modelAndMaterials_.Item1, value.Value.Item2),
                }
                : null;
        SelectedMaterialsService.SelectMaterial(field?.Item2);
      }
    }

    public MaterialPanelViewModel? SelectedMaterialPanel {
      get;
      private set => this.RaiseAndSetIfChanged(
          ref field,
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