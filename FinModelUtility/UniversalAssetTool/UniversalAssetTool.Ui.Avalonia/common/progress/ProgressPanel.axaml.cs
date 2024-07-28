using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.progress;

public class ProgressPanelViewModelForDesigner
    : ProgressPanelViewModel {
  public ProgressPanelViewModelForDesigner() {
    this.Progress = new ValueFractionProgress();

    var secondsToWait = 3;
    var start = DateTime.Now;

    Task.Run(
        async () => {
          DateTime current;
          double elapsedSeconds;
          do {
            current = DateTime.Now;
            elapsedSeconds = (current - start).TotalSeconds;
            this.Progress.ReportProgress(
                100 *
                Math.Clamp((float) (elapsedSeconds / secondsToWait), 0, 1));

            await Task.Delay(50);
          } while (elapsedSeconds < secondsToWait);

          this.Progress.ReportCompletion("Hello world!");
        });
  }
}

public class ProgressPanelViewModel : ViewModelBase {
  private ValueFractionProgress progress_;
  private ProgressSpinnerViewModel progressSpinner_;
  private IDataTemplate dataTemplate_;

  public ValueFractionProgress Progress {
    get => this.progress_;
    set {
      this.RaiseAndSetIfChanged(ref this.progress_, value);
      this.ProgressSpinner = new ProgressSpinnerViewModel {
          Progress = value
      };
    }
  }

  public ProgressSpinnerViewModel ProgressSpinner {
    get => this.progressSpinner_;
    private set
      => this.RaiseAndSetIfChanged(ref this.progressSpinner_, value);
  }

  public IDataTemplate DataTemplate {
    get => this.dataTemplate_;
    set => this.RaiseAndSetIfChanged(ref this.dataTemplate_, value);
  }
}

public partial class ProgressPanel : UserControl {
  private IDataTemplate dataTemplate_;

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