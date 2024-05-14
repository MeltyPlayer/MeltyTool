using System;
using System.Collections.ObjectModel;

using Avalonia.Controls;

using fin.audio.io;
using fin.audio.io.importers.ogg;
using fin.image;
using fin.importers;
using fin.io;
using fin.io.bundles;
using fin.model.io;
using fin.scene;
using fin.util.asserts;

using grezzo.api;

using uni.ui.avalonia.resources;
using uni.ui.avalonia.ViewModels;

using IImage = Avalonia.Media.IImage;

namespace uni.ui.avalonia.common.treeViews {
  public partial class FilterTreeView : UserControl {
    public FilterTreeView() {
      this.InitializeComponent();
      this.DataContext = new FileBundleTreeViewModel();
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
}