using System.Collections.Generic;
using System.Windows.Forms;

using fin.model;

namespace uni.ui.winforms.right_panel.materials {
  public partial class MaterialsTab : UserControl {
    private IReadOnlyModel? model_;

    public MaterialsTab() {
      InitializeComponent();

      this.materialSelector_.OnMaterialSelected += material => {
        this.materialViewerPanel1.Material = material;
        this.shaderSection_.ModelAndMaterial =(this.model_!, material);
        this.textureSection_.Material = material;
      };
    }

    public (IReadOnlyModel, IReadOnlyList<IReadOnlyMaterial>)? ModelAndMaterials {
      set {
        this.model_ = value?.Item1;
        this.materialSelector_.Materials = value?.Item2;
      }
    }
  }
}