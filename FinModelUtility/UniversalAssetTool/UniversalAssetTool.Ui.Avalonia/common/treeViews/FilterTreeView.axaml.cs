using Avalonia.Controls;

using fin.util.asserts;

namespace uni.ui.avalonia.common.treeViews;

public partial class FilterTreeView : UserControl {
  public FilterTreeView() {
    this.InitializeComponent();
    this.DataContext = new FileBundleTreeViewModelForDesigner();

    this.autocompleteTextbox_.TextChanged
        += this.AutocompleteTextbox_OnTextChanged;
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

  private void AutocompleteTextbox_OnTextChanged(
      object? sender,
      TextChangedEventArgs e) {
    if (this.DataContext is not IFilterTreeViewViewModel
        filterTreeViewViewModel) {
      return;
    }

    filterTreeViewViewModel.UpdateFilter(this.autocompleteTextbox_.Text);
  }
}