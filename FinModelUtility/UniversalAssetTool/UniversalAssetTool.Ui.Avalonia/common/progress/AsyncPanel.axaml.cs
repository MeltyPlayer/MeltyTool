using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using fin.util.asserts;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class AsyncPanelViewModelForDesigner
    : AsyncPanelViewModel {
  public AsyncPanelViewModelForDesigner() {
    this.Progress = new AsyncProgress();

    var secondsToWait = 3;
    var start = DateTime.Now;

    Task.Run(
        async () => {
          DateTime current;
          double elapsedSeconds;
          do {
            current = DateTime.Now;
            elapsedSeconds = (current - start).TotalSeconds;
            await Task.Delay(50);
          } while (elapsedSeconds < secondsToWait);

          this.Progress.ReportCompletion("Hello world!");
        });
  }
}

public class AsyncPanelViewModel : ViewModelBase {
  private AsyncProgress progress_;
  private IDataTemplate dataTemplate_;

  public AsyncProgress Progress {
    get => this.progress_;
    set => this.RaiseAndSetIfChanged(ref this.progress_, value);
  }

  public IDataTemplate DataTemplate {
    get => this.dataTemplate_;
    set => this.RaiseAndSetIfChanged(ref this.dataTemplate_, value);
  }
}

public partial class AsyncPanel : UserControl {
  public AsyncPanel() {
    InitializeComponent();
  }

  private AsyncPanelViewModel ViewModel_
    => Asserts.AsA<AsyncPanelViewModel>(this.DataContext);

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
    get => this.ViewModel_.DataTemplate;
    set {
      var dataTemplate = this.DataTemplate;
      this.ViewModel_.DataTemplate = value;
      this.SetAndRaise(DataTemplateProperty, ref dataTemplate, value);
    }
  }
}