using System.Windows.Forms;

using fin.animation;
using fin.io.bundles;
using fin.model;

using uni.ui.winforms.right_panel.skeleton;

namespace uni.ui.winforms.right_panel {
  public partial class ModelTabs : UserControl {
    public ModelTabs() {
      InitializeComponent();
    }

    public (IFileBundle, IReadOnlyModel)? Model {
      set {
        var modelFileBundle = value?.Item1;
        var model = value?.Item2;

        this.infoTab_.FileBundle = modelFileBundle;
        this.animationsTab_.Model = model;
        this.materialsTab_.ModelAndMaterials =
            model != null ? (model, model.MaterialManager.All) : null;
        this.registersTab_.Model = model;
        this.skeletonTab_.Model = model;
        this.texturesTab_.Model = model;
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
}
