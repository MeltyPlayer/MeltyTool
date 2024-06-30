using System.Collections.Generic;
using System.Linq;

using fin.io;
using fin.io.bundles;
using fin.util.linq;

using uni.ui.common;


namespace uni.ui.winforms.common.fileTreeView;

public abstract partial class FileTreeView<TFiles> {
  protected class ParentFileNode : BFileNode, IFileTreeParentNode {
    public ParentFileNode(FileTreeView<TFiles> treeView) : base(
        treeView) {
        this.InitializeFilterNode(treeView.filterImpl_.Root);
        this.InitDirectory_();
      }

    private ParentFileNode(ParentFileNode parent, string text) : base(
        parent,
        text) {
        this.InitializeFilterNode(parent);
        this.InitDirectory_();
      }

    private void InitDirectory_() {
        this.treeNode.ClosedImage = Icons.folderClosedImage;
        this.treeNode.OpenImage = Icons.folderOpenImage;
      }

    public IFileHierarchyDirectory? Directory { get; set; }
    public override string? FullName => this.Directory?.FullPath;

    public ParentFileNode AddChild(string text) => new(this, text);

    public LeafFileNode AddChild(IAnnotatedFileBundle file,
                                 string? text = null)
      => new(this, file, text);

    public IEnumerable<IFileTreeNode> ChildNodes
      => this.filterNode.Children.Select(fuzzyNode => fuzzyNode.Data);

    public IEnumerable<IAnnotatedFileBundle> GetFiles(bool recursive) {
        var children = this.ChildNodes.OfType<IFileTreeLeafNode>()
                           .Select(fileNode => fileNode.File);
        return !recursive
            ? children
            : children.Concat(
                this.ChildNodes
                    .OfType<IFileTreeParentNode>()
                    .SelectMany(parentNode
                                    => parentNode
                                        .GetFiles(
                                            true)));
      }

    public IEnumerable<IAnnotatedFileBundle<TSpecificFile>> GetFilesOfType<
        TSpecificFile>(bool recursive) where TSpecificFile : IFileBundle
      => this.GetFiles(recursive)
             .SelectWhere<IAnnotatedFileBundle,
                 IAnnotatedFileBundle<TSpecificFile>>(
                 AnnotatedFileBundleExtensions.IsOfType);

    public void Expand() => this.treeNode.Expand();
  }
}