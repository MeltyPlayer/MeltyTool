using ReactiveUI;

using uni.ui.avalonia.model;

namespace uni.ui.avalonia.ViewModels;

public class MainViewModel : ViewModelBase {
  private ModelPanelViewModel modelPanel_;

  public MainViewModel() {
    this.ModelPanel = new ModelPanelViewModel();
    ModelService.OnModelOpened
        += (_, model) => {
             this.ModelPanel = new ModelPanelViewModel { Model = model };
           };
  }

  public ModelPanelViewModel ModelPanel {
    get => this.modelPanel_;
    private set => this.RaiseAndSetIfChanged(
        ref this.modelPanel_,
        value);
  }
}