using Avalonia.Controls;

using fin.util.asserts;

namespace fin.ui.avalonia.controls;

public abstract class BControl<TViewModel> : Control
    where TViewModel : IViewModelBase {
  public TViewModel ViewModel {
    get => this.DataContext.AssertAsA<TViewModel>();
    set => this.DataContext = value.AssertAsA<TViewModel>();
  }
}

public abstract class BUserControl<TViewModel> : UserControl
    where TViewModel : IViewModelBase {
  public TViewModel ViewModel {
    get => this.DataContext.AssertAsA<TViewModel>();
    set => this.DataContext = value.AssertAsA<TViewModel>();
  }
}

public abstract class BWindow<TViewModel> : Window
    where TViewModel : IViewModelBase {
  public TViewModel ViewModel {
    get => this.DataContext.AssertAsA<TViewModel>();
    set => this.DataContext = value.AssertAsA<TViewModel>();
  }
}