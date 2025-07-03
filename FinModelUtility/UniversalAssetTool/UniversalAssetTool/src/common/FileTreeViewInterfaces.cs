using System.Drawing;

using fin.io.bundles;
using fin.util.linq;

namespace uni.ui.winforms.common.fileTreeView;

public interface IFileTreeView {
  public delegate void FileSelectedHandler(IFileTreeLeafNode fileNode);

  event FileSelectedHandler FileSelected;


  public delegate void DirectorySelectedHandler(
      IFileTreeParentNode directoryNode);

  event DirectorySelectedHandler DirectorySelected;

  Image GetImageForFile(IFileBundle file);
}

public interface IFileTreeNode {
  string Text { get; }
  IFileTreeParentNode? Parent { get; }
}

public interface IFileTreeParentNode : IFileTreeNode {
  IEnumerable<IFileTreeNode> ChildNodes { get; }

  IEnumerable<IAnnotatedFileBundle> GetFiles(bool recursive) {
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

  IEnumerable<IAnnotatedFileBundle<TSpecificFile>> GetFilesOfType<
      TSpecificFile>(bool recursive) where TSpecificFile : IFileBundle
    => this.GetFiles(recursive)
           .SelectWhere<IAnnotatedFileBundle,
               IAnnotatedFileBundle<TSpecificFile>>(
               AnnotatedFileBundleExtensions.IsOfType);
}

public interface IFileTreeLeafNode : IFileTreeNode {
  IAnnotatedFileBundle File { get; }
}

public static class FileTreeExtensions {
  public static string GetLocalPath(this IFileTreeNode node) {
    var localPath = "";
    while (node != null) {
      localPath = node is IFileTreeLeafNode
          ? node.Text
          : $"{node.Text}/{localPath}";

      node = node.Parent;
    }

    return $"//{localPath}";
  }
}