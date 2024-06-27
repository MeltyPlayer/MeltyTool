using System.Linq;
using System.Windows.Forms;

using fin.animation;
using fin.model;

namespace uni.ui.winforms.right_panel {
  public partial class AnimationsTab : UserControl {
    private IReadOnlyModelAnimation[]? animations_;

    public AnimationsTab() {
      InitializeComponent();

      this.listView_.SelectedIndexChanged += (_, e) => {
        var selectedIndices = this.listView_.SelectedIndices;

        var selectedAnimation = selectedIndices.Count > 0
                                    ? this.animations_[selectedIndices[0]]
                                    : null;

        this.OnAnimationSelected?.Invoke(selectedAnimation);
      };
    }

    public IReadOnlyModel? Model {
      set {
        this.listView_.SelectedIndices.Clear();
        this.listView_.Items.Clear();

        this.animations_ =
            value?.AnimationManager.Animations.OrderBy(
                     animation => animation.Name)
                 .ToArray();

        if (this.animations_ == null) {
          return;
        }

        foreach (var animation in this.animations_) {
          this.listView_.Items.Add(animation.Name);
        }

        if (this.listView_.Items.Count > 0) {
          this.listView_.Items[0].Selected = true;
        }
      }
    }

    public IAnimationPlaybackManager? AnimationPlaybackManager {
      get => this.animationPlaybackPanel_.Impl;
      set => this.animationPlaybackPanel_.Impl = value;
    }

    public delegate void AnimationSelected(IReadOnlyModelAnimation? animation);

    public event AnimationSelected OnAnimationSelected;
  }
}