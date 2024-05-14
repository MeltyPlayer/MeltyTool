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

  // Top-level view model types
  public class FileBundleTreeViewModel<T>
      : ViewModelBase, IFilterTreeViewViewModel<T> {
    public ObservableCollection<INode<T>> Nodes { get; protected set; }

    public event EventHandler<INode<T>>? NodeSelected;

    public void ChangeSelection(INode node)
      => this.NodeSelected?.Invoke(this, Asserts.AsA<INode<T>>(node));
  }

  public class FileBundleTreeViewModel : FileBundleTreeViewModel<IAnnotatedFileBundle> {
    public FileBundleTreeViewModel() {
      this.Nodes = [
          new ParentNode("Animals",
          [
              new ParentNode("Mammals",
              [
                  new LeafNode("Lion", new CmbModelFileBundle("foo", new FinFile()).Annotate(null)),
                  new LeafNode("Cat", new OggAudioFileBundle(new FinFile()).Annotate(null))
              ])
          ])
      ];
    }
  }

  // Node types
  public class ParentNode(
      string label,
      ObservableCollection<INode<IAnnotatedFileBundle>>? subNodes = null)
      : ViewModelBase, INode<IAnnotatedFileBundle> {
    public ObservableCollection<INode<IAnnotatedFileBundle>>? SubNodes { get; }
      = subNodes;

    public IImage? Icon => null;

    public string Label { get; } = label;
  }

  public class LeafNode(string label, IAnnotatedFileBundle data)
      : ViewModelBase, INode<IAnnotatedFileBundle> {
    public ObservableCollection<INode<IAnnotatedFileBundle>>? SubNodes => null;

    public static readonly IImage MUSIC_ICON
        = EmbeddedResourceUtil.LoadAvaloniaImage("music");

    public static readonly IImage PICTURE_ICON
        = EmbeddedResourceUtil.LoadAvaloniaImage("picture");

    public static readonly IImage MODEL_ICON
        = EmbeddedResourceUtil.LoadAvaloniaImage("model");

    public static readonly IImage SCENE_ICON
        = EmbeddedResourceUtil.LoadAvaloniaImage("scene");

    public IImage? Icon => data.FileBundle switch {
        IAudioFileBundle => MUSIC_ICON,
        IImageFileBundle => PICTURE_ICON,
        IModelFileBundle => MODEL_ICON,
        ISceneFileBundle => SCENE_ICON,
    };

    public string Label { get; } = label;
  }
}