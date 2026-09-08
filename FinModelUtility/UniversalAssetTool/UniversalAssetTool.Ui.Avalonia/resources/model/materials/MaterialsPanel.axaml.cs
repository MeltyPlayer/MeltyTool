using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Data.Converters;

using fin.data.dictionaries;
using fin.model;
using fin.ui;
using fin.ui.rendering;
using fin.util.strings;

using ReactiveUI;

namespace uni.ui.avalonia.resources.model.materials;

using MaterialTuple = (int index, IReadOnlyModel model, IReadOnlyMaterial? material);

public sealed class MaterialsPanelViewModelForDesigner
    : MaterialsPanelViewModel {
  public MaterialsPanelViewModelForDesigner() {
    var (model, material) = ModelDesignerUtil.CreateStubModelAndMaterial();

    var modelsAndMaterials = new ListDictionary<IReadOnlyModel, IReadOnlyMaterial?>();
    modelsAndMaterials.Add(model, material);
    modelsAndMaterials.Add(model, material);
    modelsAndMaterials.Add(model, material);

    this.ModelsAndMaterials = modelsAndMaterials;
  }
}

public class MaterialsPanelViewModel : BViewModel {
  public IReadOnlyListDictionary<IReadOnlyModel, IReadOnlyMaterial?>
      ModelsAndMaterials {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      var allMaterials = new List<MaterialTuple>(value.TotalCount);
      foreach (var model in value.Keys) {
        var materials = value[model];
        allMaterials.AddRange(
            materials.OrderBy(m => m?.Name, StringUtil.NaturalSortInstance)
                     .Select((m, i) => (i, model, m)));
      }

      this.Materials
          = new ObservableCollection<MaterialTuple>(
              allMaterials.OrderBy(t => t.Item3?.Name,
                                   StringUtil.NaturalSortInstance));
    }
  }

  public ObservableCollection<MaterialTuple> Materials {
    get;
    private set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.SelectedMaterial = this.Materials.FirstOrDefault();
    }
  }

  public MaterialTuple? SelectedMaterial {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.SelectedMaterialPanel
          = value != null
              ? new MaterialPanelViewModel {
                  ModelAndMaterial = (value.Value.model, value.Value.material),
              }
              : null;
      SelectedMaterialsService.SelectMaterial(field?.material);
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
      new FuncValueConverter<MaterialTuple,
              string>(t => $"Material {t.index}: {(t.material?.Name ?? "(null)")}");
}