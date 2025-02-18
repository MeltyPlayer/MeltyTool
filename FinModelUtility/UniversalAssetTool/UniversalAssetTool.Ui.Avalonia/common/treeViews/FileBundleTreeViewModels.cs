using System;
using System.Collections.Generic;
using System.Linq;

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

using ObservableCollections;

using uni.ui.avalonia.ViewModels;
using uni.ui.winforms.common.fileTreeView;

namespace uni.ui.avalonia.common.treeViews;

using IFileBundleNode = INode<IAnnotatedFileBundle>;

// Top-level view model types
public class FileBundleTreeViewModel
    : ViewModelBase, IFilterTreeViewViewModel<IAnnotatedFileBundle> {
  private readonly IReadOnlyList<IFileBundleNode> nodes_;

  private readonly ISynchronizedView<IFileBundleNode, IFileBundleNode>
      filteredNodes_;

  public FileBundleTreeViewModel(IReadOnlyList<IFileBundleNode> nodes) {
    this.nodes_ = nodes;

    var obsList = new ObservableList<IFileBundleNode>(nodes);
    this.filteredNodes_ = obsList.CreateView(t => t);
    this.FilteredNodes = this.filteredNodes_.ToNotifyCollectionChanged();
  }

  public INotifyCollectionChangedSynchronizedViewList<IFileBundleNode>
      FilteredNodes { get; }

  public event EventHandler<IFileBundleNode>? NodeSelected;

  public void ChangeSelection(INode node)
    => this.NodeSelected?.Invoke(this, Asserts.AsA<IFileBundleNode>(node));

  public void UpdateFilter(string? text) {
    var filter = FileBundleFilter.FromText(text);

    foreach (var node in this.nodes_) {
      node.Filter = filter;
    }

    if (filter == null) {
      this.filteredNodes_.ResetFilter();
      return;
    }

    this.filteredNodes_.AttachFilter(n => n.InFilter);
  }
}

public class FileBundleTreeViewModelForDesigner()
    : FileBundleTreeViewModel([
        new FileBundleDirectoryNode("Animals",
        [
            new FileBundleDirectoryNode("Mammals",
            [
                new FileBundleLeafNode("Lion",
                                       new CmbModelFileBundle(
                                           "foo",
                                           new FinFile()).Annotate(null)),
                new FileBundleLeafNode("Cat",
                                       new OggAudioFileBundle(new FinFile())
                                           .Annotate(null))
            ])
        ])
    ]);

// Node types
public abstract class BFileBundleNode(string text) : ViewModelBase, IFileTreeNode {
  public string Text => text;
  public IFileTreeParentNode? Parent => null;
}

public class FileBundleDirectoryNode
    : BFileBundleNode, IFileBundleNode, IFileTreeParentNode {
  private readonly IReadOnlyList<IFileBundleNode>? subNodes_;

  private readonly ISynchronizedView<IFileBundleNode,
      IFileBundleNode>? filteredSubNodes_;

  public FileBundleDirectoryNode(
      string label,
      IReadOnlyList<IFileBundleNode>? subNodes) : this(label,
    subNodes,
    new HashSet<string>([label])) { }

  public FileBundleDirectoryNode(
      string label,
      IReadOnlyList<IFileBundleNode>? subNodes,
      IReadOnlySet<string> filterTerms) : base(label) {
    this.subNodes_ = subNodes;
    this.Label = label;
    this.FilterTerms = filterTerms;

    var obsList = subNodes != null
        ? new ObservableList<IFileBundleNode>(subNodes)
        : null;
    this.filteredSubNodes_ = obsList?.CreateView(t => t);
    this.FilteredSubNodes = this.filteredSubNodes_?.ToNotifyCollectionChanged();
  }

  public IAnnotatedFileBundle? Value => null;

  public INotifyCollectionChangedSynchronizedViewList<
      IFileBundleNode>? FilteredSubNodes { get; }

  public MaterialIconKind? Icon => null;
  public string Label { get; }
  public IReadOnlySet<string> FilterTerms { get; }

  public IFilter<IAnnotatedFileBundle>? Filter {
    set {
      if (this.subNodes_ == null || this.FilteredSubNodes == null) {
        return;
      }

      foreach (var node in this.subNodes_) {
        node.Filter = value;
      }

      if (value == null) {
        this.filteredSubNodes_?.ResetFilter();
        this.InFilter = true;
        return;
      }

      this.filteredSubNodes_?.AttachFilter(n => n.InFilter);
      this.InFilter = this.subNodes_.Any(n => n.InFilter);
    }
  }

  public bool InFilter { get; private set; } = true;

  public IEnumerable<IFileTreeNode> ChildNodes
    => this.subNodes_?.Cast<IFileTreeNode>() ?? [];
}

public class FileBundleLeafNode(string label,
                                IAnnotatedFileBundle data)
    : BFileBundleNode(label), IFileBundleNode, IFileTreeLeafNode {
  public INotifyCollectionChangedSynchronizedViewList<
      IFileBundleNode>? FilteredSubNodes => null;

  public MaterialIconKind? Icon => data.FileBundle switch {
      IAudioFileBundle => MaterialIconKind.VolumeHigh,
      IImageFileBundle => MaterialIconKind.ImageOutline,
      IModelFileBundle => MaterialIconKind.CubeOutline,
      ISceneFileBundle => MaterialIconKind.Web,
  };

  public IAnnotatedFileBundle Value => data;
  public IAnnotatedFileBundle File => data;
  public string Label => label;

  public IFilter<IAnnotatedFileBundle>? Filter {
    set => this.InFilter = value?.MatchesNode(this) ?? true;
  }

  public bool InFilter { get; private set; } = true;
}

public class FileBundleFilter(IReadOnlySet<string> tokens)
    : IFilter<IAnnotatedFileBundle> {
  public static FileBundleFilter? FromText(string? text) {
    var tokens = text?.Split([' ', '\t', '\n'],
                             StringSplitOptions.RemoveEmptyEntries |
                             StringSplitOptions.TrimEntries);
    return tokens?.Length > 0
        ? new FileBundleFilter(new HashSet<string>(tokens))
        : null;
  }

  public bool MatchesNode(IFileBundleNode node) {
    foreach (var token in tokens) {
      var fileBundle = node.Value.FileBundle;

      if (this.ContainsToken_(node.Label, token)) {
        goto FoundMatch;
      }

      if (fileBundle.GameName != null &&
          this.ContainsToken_(fileBundle.GameName, token)) {
        goto FoundMatch;
      }

      foreach (var file in fileBundle.Files) {
        if (this.ContainsToken_(file.DisplayFullPath, token)) {
          goto FoundMatch;
        }
      }

      return false;

      FoundMatch: ;
    }

    return true;
  }

  private bool ContainsToken_(string text, string token)
    => text.Contains(token, StringComparison.OrdinalIgnoreCase);
}