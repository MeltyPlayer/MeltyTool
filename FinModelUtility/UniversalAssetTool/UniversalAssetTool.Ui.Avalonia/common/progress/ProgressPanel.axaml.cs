using System;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using fin.util.asserts;

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
  public ProgressPanel() {
      InitializeComponent();
    }

  private ProgressPanelViewModel ViewModel_
    => Asserts.AsA<ProgressPanelViewModel>(this.DataContext);

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