using Avalonia.Controls;

using fin.animation;
using fin.model;
using fin.ui.rendering;

using ReactiveUI;

using uni.ui.avalonia.resources.animation;
using uni.ui.avalonia.resources.registers;
using uni.ui.avalonia.resources.model.materials;
using uni.ui.avalonia.resources.model.mesh;
using uni.ui.avalonia.resources.model.skeleton;
using uni.ui.avalonia.resources.texture;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.model {
  public class ModelPanelViewModelForDesigner : ModelPanelViewModel {
    public ModelPanelViewModelForDesigner() {
      this.Model = ModelDesignerUtil.CreateStubModel();
    }
  }

  public class ModelPanelViewModel : ViewModelBase {
    public IReadOnlyModel Model {
      get;
      set {
        this.RaiseAndSetIfChanged(ref field, value);
        this.AnimationsPanel = new AnimationsPanelViewModel {
            Animations = value.AnimationManager.Animations,
            AnimationPlaybackManager = new FrameAdvancer {
                LoopPlayback = true,
            }
        };
        this.MaterialsPanel = new MaterialsPanelViewModel {
            ModelAndMaterials = (value, value.MaterialManager.All)
        };
        this.MeshList = new MeshListViewModel { Meshes = value.Skin.Meshes };
        this.FilesPanel = new FilesPanelViewModel(value);
        this.RegistersPanel = new RegistersPanelViewModel() {
            Registers = value.MaterialManager.Registers,
        };
        this.SkeletonTree = new SkeletonTreeViewModel {
            Skeleton = value.Skeleton,
        };
        this.TexturesPanel = new TexturesPanelViewModel {
            ModelAndTextures = (value, value.MaterialManager.Textures),
        };
      }
    }

    public AnimationsPanelViewModel AnimationsPanel {
      get;
      private set
        => this.RaiseAndSetIfChanged(ref field, value);
    }

    public FilesPanelViewModel FilesPanel {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public MaterialsPanelViewModel MaterialsPanel {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public MeshListViewModel MeshList {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public RegistersPanelViewModel RegistersPanel {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public SkeletonTreeViewModel SkeletonTree {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TexturesPanelViewModel TexturesPanel {
      get;
      private set => this.RaiseAndSetIfChanged(ref field, value);
    }
  }

  public partial class ModelPanel : UserControl {
    public ModelPanel() {
      this.InitializeComponent();
    }

    private void ClearSelectedTextureWhenTabChanged_(
        object? sender,
        SelectionChangedEventArgs e) {
      if (e.Source != this.ModelTabs) {
        return;
      }

      var shouldDeselectSkeleton = true;
      var shouldDeselectMesh = true;
      var shouldDeselectTexture = true;
      if (e.AddedItems.Count > 0) {
        if (e.AddedItems[0] is TabItem item) {
          var header = item.Header;

          if (header == this.SkeletonTabHeader) {
            shouldDeselectSkeleton = true;
          }

          if (header == this.MeshesTabHeader) {
            shouldDeselectMesh = true;
          }

          if (header == this.MaterialsTabHeader ||
              header == this.TexturesTabHeader) {
            shouldDeselectTexture = false;
          }
        }
      }

      if (shouldDeselectSkeleton) {
        SelectedBoneService.SelectBone(null);
      }

      if (shouldDeselectMesh) {
        SelectedMeshService.SelectMesh(null);
      }

      if (shouldDeselectTexture) {
        SelectedTextureService.SelectTexture(null);
      }
    }
  }
}