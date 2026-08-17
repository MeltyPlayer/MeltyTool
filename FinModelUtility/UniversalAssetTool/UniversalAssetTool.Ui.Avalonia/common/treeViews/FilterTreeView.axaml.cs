using Avalonia.Controls;

using fin.ui.avalonia.controls;

namespace uni.ui.avalonia.common.treeViews;

public partial class FilterTreeView : BUserControl<IFilterTreeViewViewModel> {
  public FilterTreeView() {
    this.InitializeComponent();
    this.ViewModel = new FileBundleTreeViewModelForDesigner();

    this.autocompleteTextbox_.TextChanged
        += this.AutocompleteTextbox_OnTextChanged;
  }

  private void AutocompleteTextbox_OnTextChanged(
      object? sender,
      TextChangedEventArgs e)
    => this.ViewModel.UpdateFilter(this.autocompleteTextbox_.Text);
}