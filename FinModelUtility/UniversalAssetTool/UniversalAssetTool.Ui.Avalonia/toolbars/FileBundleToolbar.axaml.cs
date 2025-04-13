using Avalonia.Controls;

using fin.io.bundles;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.toolbars;

public class FileBundleToolbarModelForDesigner : FileBundleToolbarModel {
  public string FileName => "//foo/bar.mod";
  public IFileBundle? FileBundle => null;
}

public class FileBundleToolbarModel : ViewModelBase {
  public string? FileName {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public IFileBundle? FileBundle {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class FileBundleToolbar : UserControl {
  public FileBundleToolbar() => this.InitializeComponent();
}