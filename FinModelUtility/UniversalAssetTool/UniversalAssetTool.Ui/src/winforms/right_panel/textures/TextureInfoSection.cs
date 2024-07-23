using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Forms;

using fin.image;
using fin.model;
using fin.util.image;

namespace uni.ui.winforms.right_panel.textures;

public partial class TextureInfoSection : UserControl {
  public TextureInfoSection() {
      this.InitializeComponent();

      this.SelectedTexture = null;
    }

  public IReadOnlyTexture? SelectedTexture {
    set => this.propertyGrid_.SelectedObject =
        new PropertyGridTexture(value);
  }

  private class PropertyGridTexture(IReadOnlyTexture? impl) {
    [Display(Order = 0)]
    [Category("Metadata")]
    public string? Name => impl?.Name;

    [Display(Order = 1)]
    [Category("Metadata")]
    public PixelFormat? PixelFormat => impl?.Image.PixelFormat;

    [Display(Order = 2)]
    [Category("Metadata")]
    public TransparencyType? TransparencyType
      => impl != null
          ? TransparencyTypeUtil.GetTransparencyType(impl.Image)
          : null;

    [Display(Order = 3)]
    [Category("Metadata")]
    public int? Width => impl?.Image.Width;
 
    [Display(Order = 4)]
    [Category("Metadata")]
    public int? Height => impl?.Image.Height;

    [Display(Order = 5)]
    [Category("Metadata")]
    public WrapMode? HorizontalWrapMode => impl?.WrapModeU;

    [Display(Order = 6)]
    [Category("Metadata")]
    public WrapMode? VerticalWrapMode => impl?.WrapModeV;

    [Display(Order = 7)]
    [Category("Metadata")]
    public UvType? UvType => impl?.UvType;
  }
}