using System.Windows.Forms;

using fin.model;
using fin.shaders.glsl;

namespace uni.ui.winforms.right_panel.materials;

public partial class ShaderSection : UserControl {
  public ShaderSection() {
    this.InitializeComponent();
    }

  public (IReadOnlyModel, IReadOnlyMaterial?)? ModelAndMaterial {
    set {
        if (value == null) {
          this.richTextBox_.Text = "(n/a)";
        } else {
          var (model, material) = value.Value;
          this.richTextBox_.Text =
              material.ToShaderSource(model, false).FragmentShaderSource;
        }
      }
  }
}