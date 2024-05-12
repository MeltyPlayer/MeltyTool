using ReactiveUI;

namespace uni.ui.avalonia.ViewModels;

public interface IViewModelBase
    : IReactiveNotifyPropertyChanged<IReactiveObject>,
      IHandleObservableErrors,
      IReactiveObject;

public class ViewModelBase : ReactiveObject, IViewModelBase;