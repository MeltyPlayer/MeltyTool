using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Media;

using uni.ui.avalonia.resources;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.treeViews {
  public partial class FilterTreeView : UserControl {
    public FilterTreeView() {
      InitializeComponent();
      this.DataContext = new FilterTreeViewViewModel();
    }
  }

  public class FilterTreeViewViewModel : FilterTreeViewViewModel<string> {
    public FilterTreeViewViewModel() {
      this.Nodes = [
          new ParentNode<string>("Animals",
          [
              new ParentNode<string>("Mammals",
              [
                  new ParentNode<string>("Lion"),
                  new ParentNode<string>("Cat"),
                  new ParentNode<string>("Zebra")
              ])
          ])
      ];
    }
  }

  public class FilterTreeViewViewModel<T> : ViewModelBase {
    public ObservableCollection<INode<T>> Nodes { get; protected set; }
  }

  public interface INode<T> : IViewModelBase {
    ObservableCollection<INode<T>>? SubNodes { get; }
    IImage Icon { get; }
    string Label { get; }
  }

  public class ParentNode<T>(
      string label,
      ObservableCollection<INode<T>>? subNodes = null)
      : ViewModelBase, INode<T> {
    public ObservableCollection<INode<T>>? SubNodes { get; } = subNodes;

    public IImage Icon { get; }
      = EmbeddedResourceUtil.LoadAvaloniaImage("folder_open");
    public string Label { get; } = label;
  }
}