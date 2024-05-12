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
    bool IsExpanded { get; set; }
    ObservableCollection<INode<T>>? SubNodes { get; }
    IImage Icon { get; }
    string Label { get; }
  }

  public class ParentNode<T>(
      string label,
      ObservableCollection<INode<T>>? subNodes = null)
      : ViewModelBase, INode<T> {
    public ObservableCollection<INode<T>>? SubNodes { get; } = subNodes;

    public static readonly IImage FOLDER_OPEN_ICON
        = EmbeddedResourceUtil.LoadAvaloniaImage("folder_open");

    public static readonly IImage FOLDER_CLOSED_ICON
        = EmbeddedResourceUtil.LoadAvaloniaImage("folder_closed");

    public bool IsExpanded { get; set; }

    public IImage Icon
      => this.IsExpanded ? FOLDER_OPEN_ICON : FOLDER_CLOSED_ICON;

    public string Label { get; } = label;
  }
}