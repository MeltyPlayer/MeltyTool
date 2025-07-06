using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class ProgressPanelViewModelForDesigner
    : ProgressPanelViewModel {
  public ProgressPanelViewModelForDesigner() {
    this.Progress = ValueFractionProgress.FromTimer(3, "Hello world!");
  }
}

public class ProgressPanelViewModel : ViewModelBase {
  public ValueFractionProgress Progress {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.ProgressSpinner = new ProgressSpinnerViewModel {
          Progress = value
      };
    }
  }

  public ProgressSpinnerViewModel ProgressSpinner {
    get;
    private set
      => this.RaiseAndSetIfChanged(ref field, value);
  }

  public IDataTemplate DataTemplate {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class ProgressPanel : UserControl {
  public ProgressPanel() {
    this.InitializeComponent();
    this.DataContextChanged += (_, _) => {
      if (this.ViewModel_ != null) {
        this.ViewModel_.DataTemplate = this.DataTemplate;
      }
    };
  }

  private ProgressPanelViewModel? ViewModel_
    => this.DataContext as ProgressPanelViewModel;

  /// <summary>
  /// Defines the <see cref="ItemTemplate"/> property.
  /// </summary>
  public static readonly DirectProperty<ProgressPanel, IDataTemplate>
      DataTemplateProperty = AvaloniaProperty
          .RegisterDirect<ProgressPanel, IDataTemplate>(
              "DataTemplate",
              owner => owner.DataTemplate,
              (owner, value) => owner.DataTemplate = value);

  public IDataTemplate DataTemplate {
    get;
    set {
      this.SetAndRaise(DataTemplateProperty, ref field, value);
      field = value;

      if (this.ViewModel_ != null) {
        this.ViewModel_.DataTemplate = value;
      }
    }
  }
}