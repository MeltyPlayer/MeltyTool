using System.Windows.Forms;

using fin.animation;
using fin.model;
using fin.shaders.glsl;

using uni.ui.winforms.right_panel.skeleton;

namespace uni.ui.winforms.right_panel;

public partial class ModelTabs : UserControl {
  public ModelTabs() {
    this.InitializeComponent();
  }

  public IReadOnlyModel? Model {
    set {
      this.infoTab_.Resource = value;
      this.animationsTab_.Model = value;
      this.materialsTab_.ModelAndMaterials =
          value != null
              ? (value, ModelRequirements.FromModel(value),
                 value.MaterialManager.All)
              : null;
      this.registersTab_.Model = value;
      this.skeletonTab_.Model = value;
      this.texturesTab_.Model = value;
    }
  }

  public IAnimationPlaybackManager? AnimationPlaybackManager {
    get => this.animationsTab_.AnimationPlaybackManager;
    set => this.animationsTab_.AnimationPlaybackManager = value;
  }

  public event AnimationsTab.AnimationSelected OnAnimationSelected {
    add => this.animationsTab_.OnAnimationSelected += value;
    remove => this.animationsTab_.OnAnimationSelected -= value;
  }

  public event SkeletonTab.BoneSelected OnBoneSelected {
    add => this.skeletonTab_.OnBoneSelected += value;
    remove => this.skeletonTab_.OnBoneSelected -= value;
  }
}