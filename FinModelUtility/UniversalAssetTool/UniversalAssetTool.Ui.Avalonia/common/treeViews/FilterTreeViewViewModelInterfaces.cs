using System;

using fin.ui.avalonia;

using Material.Icons;

using ObservableCollections;

namespace uni.ui.avalonia.common.treeViews;

// Top-level view model types

public interface IFilter<T> {
  bool MatchesNode(INode<T> node);
}

public interface IFilterTreeViewViewModel : IViewModelBase {
  void ChangeSelection(INode node);
  void UpdateFilter(string? text);
}

public interface IFilterTreeViewViewModel<T> : IFilterTreeViewViewModel {
  event EventHandler<INode<T>>? NodeSelected;
}

// Node types
public interface INode : IViewModelBase {
  MaterialIconKind? Icon { get; }
  string Label { get; }
  bool InFilter { get; }
}

public interface INode<T> : INode {
  T Value { get; }

  INotifyCollectionChangedSynchronizedViewList<INode<T>>? FilteredSubNodes {
    get;
  }

  IFilter<T>? Filter { set; }
}