using System.Windows.Forms;

using fin.model;
using fin.shaders.glsl;

namespace uni.ui.winforms.right_panel.materials;

public partial class ShaderSection : UserControl {
  public ShaderSection() {
    this.InitializeComponent();
    }

  public (IReadOnlyModel, IModelRequirements, IReadOnlyMaterial?)? ModelAndMaterial {
    set {
        if (value == null) {
          this.richTextBox_.Text = "(n/a)";
        } else {
          var (model, modelRequirements, material) = value.Value;
          this.richTextBox_.Text = material.ToShaderSource(model, modelRequirements).FragmentShaderSource;
        }
      }
  }
}