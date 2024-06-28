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
  private IDataTemplate dataTemplate_;

  public AsyncPanel() {
    InitializeComponent();
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
    get => this.dataTemplate_;
    set {
      this.SetAndRaise(DataTemplateProperty, ref this.dataTemplate_, value);
      this.dataTemplate_ = value;

      if (this.ViewModel_ != null) {
        this.ViewModel_.DataTemplate = value;
      }
    }
  }
}