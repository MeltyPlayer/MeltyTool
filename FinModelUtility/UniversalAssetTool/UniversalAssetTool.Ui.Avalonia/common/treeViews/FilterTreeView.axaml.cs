using System.Collections.ObjectModel;

using Avalonia.Controls;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.treeViews {
  public partial class FilterTreeView : UserControl {
    public FilterTreeView() {
      InitializeComponent();
    }
  }

  public class FilterTreeViewViewModel : ViewModelBase {
    public ObservableCollection<Node> Nodes { get; } = [
        new("Animals",
        [
            new Node("Mammals",
            [
                new("Lion"),
                new("Cat"),
                new("Zebra")
            ])
        ])
    ];
  }

  public class Node(string title, ObservableCollection<Node>? subNodes = null) {
    public ObservableCollection<Node>? SubNodes { get; } = subNodes;
    public string Title { get; } = title;
  }
}