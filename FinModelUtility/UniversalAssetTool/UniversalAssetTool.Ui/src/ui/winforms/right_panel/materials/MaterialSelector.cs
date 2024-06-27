using System.Collections.Generic;
using System.Windows.Forms;

using fin.model;
using fin.util.lists;

namespace uni.ui.winforms.right_panel.materials {
  public partial class MaterialSelector : UserControl {
    private IReadOnlyList<IReadOnlyMaterial>? materials_;
    private IReadOnlyMaterial? selectedMaterial_ = null;

    public MaterialSelector() {
      InitializeComponent();

      this.comboBox_.SelectedIndexChanged += (_, _) => {
        var selectedIndex = this.comboBox_.SelectedIndex;
        this.SelectedMaterial = selectedIndex == -1
                                    ? null
                                    : this.materials_?[selectedIndex];
      };
    }

    public IReadOnlyList<IReadOnlyMaterial>? Materials {
      get => this.materials_;
      set {
        this.materials_ = value;

        if (this.materials_ == null) {
          return;
        }

        var comboBoxItems = this.comboBox_.Items;
        comboBoxItems.Clear();
        for (var i = 0; i < this.materials_.Count; ++i) {
          var material = this.materials_[i];
          comboBoxItems.Add($"{i}: \"{material.Name ?? "(null)"}\"");
        }

        this.SelectedMaterial =
            this.materials_.Count > 0 ? this.materials_[0] : null;
      }
    }

    public IReadOnlyMaterial? SelectedMaterial {
      get => this.selectedMaterial_;
      set {
        if (this.selectedMaterial_ == value || this.materials_ == null) {
          return;
        }

        this.comboBox_.SelectedIndex =
            value == null
                ? -1
                : ListUtil.AssertFindFirst(
                    this.materials_, value);

        this.OnMaterialSelected(this.selectedMaterial_ = value);
      }
    }

    public delegate void OnMaterialSelectedHandler(IReadOnlyMaterial? material);

    public event OnMaterialSelectedHandler OnMaterialSelected = delegate { };
  }
}