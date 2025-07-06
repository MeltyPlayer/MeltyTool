using System.Collections.Generic;

using Avalonia.Controls;

using fin.util.progress;

using ReactiveUI;

using uni.ui.avalonia.common.progress;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.io;

public record FileBundleGathererProgressViewModel(
    string Name,
    IPercentageProgress Progress) {
  //public required IFileBundleDirectory Directory { get; init; }
}

public class FileBundleGatherersProgressViewModelForDesigner
    : FileBundleGatherersProgressViewModel {
  public FileBundleGatherersProgressViewModelForDesigner() {
    this.FileBundleGatherers = [
        new FileBundleGathererProgressViewModel(
            "Fast",
            ValueFractionProgress.FromTimer(3, "fast")
        ),
        new FileBundleGathererProgressViewModel(
            "Medium",
            ValueFractionProgress.FromTimer(6, "fast")
        ),
        new FileBundleGathererProgressViewModel(
            "Slow",
            ValueFractionProgress.FromTimer(9, "fast")
        )
    ];
  }
}

public class FileBundleGatherersProgressViewModel : ViewModelBase {
  public required IReadOnlyList<FileBundleGathererProgressViewModel>?
      FileBundleGatherers {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class FileBundleGatherersProgressWindow : Window {
  public FileBundleGatherersProgressWindow() {
    InitializeComponent();
  }
}