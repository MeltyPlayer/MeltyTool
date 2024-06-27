using System.Windows.Forms;

using fin.model;

namespace uni.ui.winforms.right_panel.materials {
  public partial class MaterialViewerPanel : UserControl, IMaterialViewerPanel {
    public MaterialViewerPanel() {
      this.InitializeComponent();
    }

    public IReadOnlyMaterial? Material {
      get => this.impl_.Material;
      set {
        this.groupBox_.Text = value == null
            ? "(Select material)"
            : (value.Name ?? "(null)");
        this.impl_.Material = value;
      } 
    }
  }
}