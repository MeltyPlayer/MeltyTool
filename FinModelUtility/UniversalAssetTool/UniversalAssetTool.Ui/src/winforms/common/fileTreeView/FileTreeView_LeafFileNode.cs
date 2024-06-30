using fin.io.bundles;


namespace uni.ui.winforms.common.fileTreeView;

public abstract partial class FileTreeView<TFiles> {
  protected class LeafFileNode : BFileNode, IFileTreeLeafNode {
    public LeafFileNode(ParentFileNode parent,
                        IAnnotatedFileBundle file,
                        string? text = null) :
        base(parent, text ?? file.FileBundle.DisplayName) {
        this.File = file;
        this.InitializeFilterNode(parent);

        this.treeNode.ClosedImage =
            this.treeNode.OpenImage =
                this.treeView.GetImageForFile(this.File.FileBundle);
      }

    public IAnnotatedFileBundle File { get; }
    public override string FullName => this.File.FileBundle.TrueFullPath;
  }
}

public static class AnnotatedFileBundleExtensions {
  public static bool IsOfType<TSpecificFile>(
      this IAnnotatedFileBundle file,
      out IAnnotatedFileBundle<TSpecificFile> outFile)
      where TSpecificFile : IFileBundle {
      if (file is IAnnotatedFileBundle<TSpecificFile>) {
        outFile = (IAnnotatedFileBundle<TSpecificFile>) file;
        return true;
      }

      outFile = default;
      return false;
    }
}