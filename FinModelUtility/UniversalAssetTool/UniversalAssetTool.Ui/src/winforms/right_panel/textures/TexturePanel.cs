using System;
using System.Collections.Generic;
using System.Windows.Forms;

using fin.model;
using fin.util.asserts;

using uni.ui.winforms.common;

namespace uni.ui.winforms.right_panel.textures;

public partial class TexturePanel : UserControl {
  public TexturePanel() {
    this.InitializeComponent();

      this.Texture = null;

      this.pictureBox_.Click += ContextMenuUtils.CreateRightClickEventHandler(
          this.GenerateContextMenuItems_);
    }

  public IReadOnlyTexture? Texture {
    set {
        this.groupBox_.Text = value?.Name ?? "(Select a texture)";
        this.pictureBox_.Image = value?.ImageData;
      }
  }

  private IEnumerable<(string, Action)> GenerateContextMenuItems_() {
      if (this.pictureBox_.Image != null) {
        yield return ("Copy image", this.CopyImageToClipboard_);
      }
    }

  private void CopyImageToClipboard_()
    => Clipboard.SetImage(this.pictureBox_.Image.AssertNonnull());
}