using Avalonia.Controls;

namespace uni.ui.avalonia.common.treeViews;

public partial class FilterTreeView : UserControl {
  public FilterTreeView() {
    this.InitializeComponent();
    this.DataContext = new FileBundleTreeViewModelForDesigner();

    this.autocompleteTextbox_.TextChanged
        += this.AutocompleteTextbox_OnTextChanged;
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