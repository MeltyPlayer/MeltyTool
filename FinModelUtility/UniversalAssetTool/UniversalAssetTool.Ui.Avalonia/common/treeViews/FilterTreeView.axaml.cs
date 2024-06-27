using Avalonia.Controls;

using fin.util.asserts;

namespace uni.ui.avalonia.common.treeViews;

public partial class FilterTreeView : UserControl {
  public FilterTreeView() {
      this.InitializeComponent();
      this.DataContext = new FileBundleTreeViewModelForDesigner();
    }

  private void TreeView_OnSelectionChanged_(
      object? sender,
      SelectionChangedEventArgs e) {
      if (e.AddedItems.Count == 0) {
        return;
      }

      if (this.DataContext is not IFilterTreeViewViewModel
          filterTreeViewViewModel) {
        return;
      }

      var selectedNode = Asserts.AsA<INode>(e.AddedItems[0]);
      filterTreeViewViewModel.ChangeSelection(selectedNode);
    }
}