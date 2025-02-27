using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;

using fin.audio.io;
using fin.audio.io.importers.ogg;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.model.io;
using fin.scene;
using fin.util.asserts;
using fin.util.sets;

using grezzo.api;

using Material.Icons;
using Material.Icons.Avalonia;

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

    var autoCompleteItems = new HashSet<string>();
    var nodeQueue = new FinQueue<IFileBundleNode>(nodes);
    while (nodeQueue.TryDequeue(out IFileBundleNode node)) {
      var file = node.Value;
      if (file != null) {
        autoCompleteItems.Add(file.FileBundle.DisplayFullPath.ToString());
      } else {
        autoCompleteItems.Add(node.Label);
      }

      if (node.FilteredSubNodes != null) {
        nodeQueue.Enqueue(node.FilteredSubNodes);
      }
    }
    this.AutoCompleteItems = autoCompleteItems.ToArray();

    var obsList = new ObservableList<IFileBundleNode>(nodes);
    this.filteredNodes_ = obsList.CreateView(t => t);

    this.Source = new HierarchicalTreeDataGridSource<IFileBundleNode>(this.filteredNodes_) {
        Columns = {
            new HierarchicalExpanderColumn<IFileBundleNode>(
                new TemplateColumn<IFileBundleNode>(
                    "Name",
                    new FuncDataTemplate<IFileBundleNode>(
                        (x, _) => {
                          if (x == null) {
                            return null;
                          }

                          var textBlock = new TextBlock {
                              Text = x.Label,
                              Classes = {"regular"}
                          };

                          if (x.Icon == null) {
                            return textBlock;
                          }

                          var icon = new MaterialIcon {
                              Kind = x.Icon.Value,
                              Margin = new Thickness(-24, 0, 4, 0),
                              Height = 16,
                              Width = 16
                          };

                          var stackPanel = new StackPanel {
                              Orientation = Orientation.Horizontal,
                          };
                          stackPanel.Children.AddRange([icon, textBlock]);

                          return stackPanel;
                        })),
                x => x.FilteredSubNodes)
        }
    };

    Dispatcher.UIThread.Invoke(() => {
      var rowSelection = this.Source.RowSelection!;
      rowSelection.SelectionChanged += (_, e) => {
        var selectedItems = e.SelectedItems;
        if (selectedItems.Count == 0) {
          return;
        }

        this.ChangeSelection(selectedItems[0]!);
      };
    });
  }

  public HierarchicalTreeDataGridSource<IFileBundleNode> Source { get; }

  public string[] AutoCompleteItems { get; }

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
    } else {
      this.filteredNodes_.AttachFilter(n => n.InFilter);
    }

    this.Source.Items = this.filteredNodes_.Where(i => i.InFilter);
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
                                           new FinFile()).Annotate(null)),
                new FileBundleLeafNode("Cat",
                                       new OggAudioFileBundle(new FinFile())
                                           .Annotate(null))
            ])
        ])
    ]);

// Node types
public abstract class BFileBundleNode(string text)
    : ViewModelBase, IFileTreeNode {
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

public class FileBundleLeafNode(
    string label,
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
      var annotatedFileBundle = node.Value;
      var fileBundle = annotatedFileBundle.FileBundle;

      if (this.ContainsToken_(node.Label, token)) {
        goto FoundMatch;
      }

      if (this.ContainsToken_(annotatedFileBundle.GameName, token)) {
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