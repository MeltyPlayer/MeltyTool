using ReactiveUI;

namespace fin.ui.avalonia;

public interface IViewModelBase
    : IReactiveNotifyPropertyChanged<IReactiveObject>,
      IHandleObservableErrors,
      IReactiveObject;

public class ViewModelBase : ReactiveObject, IViewModelBase;