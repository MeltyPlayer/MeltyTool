using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using fin.ui.avalonia;

using ReactiveUI;

namespace uni.ui.avalonia.common.progress;

public class AsyncPanelViewModelForDesigner
    : AsyncPanelViewModel {
  public AsyncPanelViewModelForDesigner() {
    this.Progress = AsyncProgress.FromTask(
        Task.Delay(3000).ContinueWith(_ => "Hello world!"));
  }
}

public class AsyncPanelViewModel : ViewModelBase {
  public AsyncProgress Progress {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public IDataTemplate DataTemplate {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class AsyncPanel : UserControl {
  public AsyncPanel() {
    this.InitializeComponent();
    this.DataContextChanged += (_, _) => {
      if (this.ViewModel_ != null) {
        this.ViewModel_.DataTemplate = this.DataTemplate;
      }
    };
  }

  private AsyncPanelViewModel? ViewModel_
    => this.DataContext as AsyncPanelViewModel;

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