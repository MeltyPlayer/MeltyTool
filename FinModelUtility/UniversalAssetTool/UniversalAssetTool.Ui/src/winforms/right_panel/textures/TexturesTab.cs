using System.Windows.Forms;

using fin.model;

namespace uni.ui.winforms.right_panel.textures;

public partial class TexturesTab : UserControl {
  public TexturesTab() {
    this.InitializeComponent();

      this.textureSelectorBox_.OnTextureSelected
          += texture => this.texturePanel_.Texture =
              this.textureInfoSection_.SelectedTexture = texture;
    }

  public IReadOnlyModel? Model {
    set => this.textureSelectorBox_.Textures =
        value?.MaterialManager.Textures;
  }
}