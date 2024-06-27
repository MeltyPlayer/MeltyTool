using System;
using System.Collections.ObjectModel;

using fin.audio.io;
using fin.audio.io.importers.ogg;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.model.io;
using fin.scene;
using fin.util.asserts;

using grezzo.api;

using Material.Icons;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.treeViews {

  // Top-level view model types
  public class FileBundleTreeViewModel<T>
      : ViewModelBase, IFilterTreeViewViewModel<T> {
    public ObservableCollection<INode<T>> Nodes { get; init; }

    public event EventHandler<INode<T>>? NodeSelected;

    public void ChangeSelection(INode node)
      => this.NodeSelected?.Invoke(this, Asserts.AsA<INode<T>>(node));
  }

  public class FileBundleTreeViewModelForDesigner : FileBundleTreeViewModel<IAnnotatedFileBundle> {
    public FileBundleTreeViewModelForDesigner() {
      this.Nodes = [
          new FileBundleDirectoryNode("Animals",
          [
              new FileBundleDirectoryNode("Mammals",
              [
                  new FileBundleLeafNode("Lion", new CmbModelFileBundle("foo", new FinFile()).Annotate(null)),
                  new FileBundleLeafNode("Cat", new OggAudioFileBundle(new FinFile()).Annotate(null))
              ])
          ])
      ];
    }
  }

  // Node types
  public class FileBundleDirectoryNode(
      string label,
      ObservableCollection<INode<IAnnotatedFileBundle>>? subNodes = null)
      : ViewModelBase, INode<IAnnotatedFileBundle> {
    public ObservableCollection<INode<IAnnotatedFileBundle>>? SubNodes { get; }
      = subNodes;

    public MaterialIconKind? Icon => null;

    public string Label { get; } = label;
  }

  public class FileBundleLeafNode(string label, IAnnotatedFileBundle data)
      : ViewModelBase, INode<IAnnotatedFileBundle> {
    public ObservableCollection<INode<IAnnotatedFileBundle>>? SubNodes => null;

    public MaterialIconKind? Icon => data.FileBundle switch {
      IAudioFileBundle => MaterialIconKind.VolumeHigh,
      IImageFileBundle => MaterialIconKind.ImageOutline,
      IModelFileBundle => MaterialIconKind.CubeOutline,
      ISceneFileBundle => MaterialIconKind.Web,
    };

    public string Label { get; } = label;

    public IAnnotatedFileBundle Data => data;
  }
}
