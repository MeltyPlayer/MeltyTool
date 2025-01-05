using System.Collections.Generic;
using System.Windows.Forms;

using fin.model;
using fin.shaders.glsl;

namespace uni.ui.winforms.right_panel.materials;

public partial class MaterialsTab : UserControl {
  private IReadOnlyModel? model_;
  private IModelRequirements modelRequirements_;

  public MaterialsTab() {
    this.InitializeComponent();

    this.materialSelector_.OnMaterialSelected += material => {
      this.shaderSection_.ModelAndMaterial
          = (this.model_!, this.modelRequirements_!, material);
      this.textureSection_.Material = material;
    };
  }

  public (IReadOnlyModel, IModelRequirements, IReadOnlyList<IReadOnlyMaterial>)?
      ModelAndMaterials {
    set {
      this.model_ = value?.Item1;
      this.modelRequirements_ = value?.Item2;
      this.materialSelector_.Materials = value?.Item3;
    }
  }
}