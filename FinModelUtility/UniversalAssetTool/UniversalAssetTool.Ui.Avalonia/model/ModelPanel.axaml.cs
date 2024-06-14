using Avalonia.Controls;

using fin.animation;
using fin.language.equations.fixedFunction;
using fin.model;

using ReactiveUI;

using uni.ui.avalonia.animations;
using uni.ui.avalonia.model.materials;
using uni.ui.avalonia.model.skeleton;
using uni.ui.avalonia.registers;
using uni.ui.avalonia.textures;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.model {
  public class ModelPanelViewModelForDesigner : ModelPanelViewModel {
    public ModelPanelViewModelForDesigner() {
      this.Model = ModelDesignerUtil.CreateStubModel();
      this.Registers = RegistersDesignerUtil.CreateStubRegisters();
    }
  }

  public class ModelPanelViewModel : ViewModelBase {
    private IReadOnlyModel model_;
    private IFixedFunctionRegisters registers_;

    private AnimationsPanelViewModel animationsPanel_;
    private MaterialsPanelViewModel materialsPanel_;
    private RegistersPanelViewModel registersPanel_;
    private SkeletonTreeViewModel skeletonTree_;
    private TexturesPanelViewModel texturesPanel_;

    public IReadOnlyModel Model {
      get => this.model_;
      set {
        this.RaiseAndSetIfChanged(ref this.model_, value);
        this.AnimationsPanel = new AnimationsPanelViewModel {
            Animations = value.AnimationManager.Animations,
            AnimationPlaybackManager = new FrameAdvancer {
                LoopPlayback = true,
            }
        };
        this.MaterialsPanel = new MaterialsPanelViewModel {
            ModelAndMaterials = (value, value.MaterialManager.All)
        };
        this.SkeletonTree = new SkeletonTreeViewModel {
            Skeleton = value.Skeleton,
        };
        this.TexturesPanel = new TexturesPanelViewModel {
            Textures = value.MaterialManager.Textures,
        };
      }
    }

    public IFixedFunctionRegisters Registers {
      get => this.registers_;
      set {
        this.RaiseAndSetIfChanged(ref this.registers_, value);
        this.RegistersPanel = new RegistersPanelViewModel {
            Registers = value,
        };
      }
    }

    public AnimationsPanelViewModel AnimationsPanel {
      get => this.animationsPanel_;
      private set
        => this.RaiseAndSetIfChanged(ref this.animationsPanel_, value);
    }

    public MaterialsPanelViewModel MaterialsPanel {
      get => this.materialsPanel_;
      private set => this.RaiseAndSetIfChanged(ref this.materialsPanel_, value);
    }

    public RegistersPanelViewModel RegistersPanel {
      get => this.registersPanel_;
      private set => this.RaiseAndSetIfChanged(ref this.registersPanel_, value);
    }

    public SkeletonTreeViewModel SkeletonTree {
      get => this.skeletonTree_;
      private set => this.RaiseAndSetIfChanged(ref this.skeletonTree_, value);
    }

    public TexturesPanelViewModel TexturesPanel {
      get => this.texturesPanel_;
      private set => this.RaiseAndSetIfChanged(ref this.texturesPanel_, value);
    }
  }

  public partial class ModelPanel : UserControl {
    public ModelPanel() {
      InitializeComponent();
    }
  }
}