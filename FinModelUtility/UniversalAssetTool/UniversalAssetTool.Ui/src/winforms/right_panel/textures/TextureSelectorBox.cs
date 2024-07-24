using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using fin.model;
using fin.util.lists;

namespace uni.ui.winforms.right_panel.textures;

public partial class TextureSelectorBox : UserControl {
  private IReadOnlyList<IReadOnlyTexture>? textures_;
  private IReadOnlyTexture? selectedTexture_;

  public TextureSelectorBox() {
      InitializeComponent();

      this.listView_.ItemSelectionChanged
          += (_, e) => { this.SelectedTexture = this.textures_?[e.ItemIndex]; };
    }

  private object listViewLock_ = new();

  public IReadOnlyList<IReadOnlyTexture>? Textures {
    set => this.listView_.Invoke(() => this.UpdateTextureListView_(value));
  }

  private void UpdateTextureListView_(
      IReadOnlyList<IReadOnlyTexture>? value) {
      lock (this.listViewLock_) {
        this.listView_.BeginUpdate();

        var imageList = this.listView_.SmallImageList
            ??= new ImageList();

        this.listView_.Clear();
        imageList.Images.Clear();

        this.textures_ =
            value?.ToHashSet(new TextureEqualityComparer())
                 .OrderBy(texture => texture.Name)
                 .ToList();
        if (this.textures_ == null) {
          this.textures_ = Array.Empty<IReadOnlyTexture>();
        }

        foreach (var texture in this.textures_) {
          this.listView_.Items.Add(texture.Name,
                                   imageList.Images.Count);
          imageList.Images.Add(texture.ImageData);
        }

        this.SelectedTexture =
            this.textures_.Count > 0 ? this.textures_[0] : null;

        this.listView_.EndUpdate();
      }
    }

  public IReadOnlyTexture? SelectedTexture {
    get => this.selectedTexture_;
    set {
        if (this.selectedTexture_ == value) {
          return;
        }

        this.selectedTexture_ = value;
        var selectedIndices = this.listView_.SelectedIndices;
        selectedIndices.Clear();
        if (value != null && this.textures_ != null) {
          selectedIndices.Add(ListUtil.AssertFindFirst(this.textures_, value));
        }

        this.OnTextureSelected(value);
      }
  }

  public delegate void OnTextureSelectedHandler(IReadOnlyTexture? texture);

  public event OnTextureSelectedHandler OnTextureSelected = delegate { };

  private class TextureEqualityComparer
      : IEqualityComparer<IReadOnlyTexture> {
    public bool Equals(IReadOnlyTexture x, IReadOnlyTexture y) {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Image.Equals(y.Image);
      }

    public int GetHashCode(IReadOnlyTexture obj) {
        return obj.Image.GetHashCode();
      }
  }
}