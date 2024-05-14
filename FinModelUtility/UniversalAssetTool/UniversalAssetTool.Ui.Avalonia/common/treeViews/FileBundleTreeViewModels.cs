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

using uni.ui.avalonia.resources;
using uni.ui.avalonia.ViewModels;

using IImage = Avalonia.Media.IImage;

namespace uni.ui.avalonia.common.treeViews {

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

    public IImage? Icon => null;

    public string Label { get; } = label;
  }

  public class FileBundleLeafNode(string label, IAnnotatedFileBundle data)
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

    public IImage Icon => data.FileBundle switch {
      IAudioFileBundle => MUSIC_ICON,
      IImageFileBundle => PICTURE_ICON,
      IModelFileBundle => MODEL_ICON,
      ISceneFileBundle => SCENE_ICON,
    };

    public string Label { get; } = label;
  }
}
