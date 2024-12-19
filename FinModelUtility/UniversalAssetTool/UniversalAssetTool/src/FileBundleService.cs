using fin.io.bundles;

using uni.ui.winforms.common.fileTreeView;

namespace uni;

public static class FileBundleService {
  static FileBundleService() {
    FileTreeLeafNodeService.OnFileTreeLeafNodeOpened += fileTreeLeafNode
        => OpenFileBundle(fileTreeLeafNode, fileTreeLeafNode.File);
  }

  public static event Action<IFileTreeLeafNode?, IAnnotatedFileBundle>
      OnFileBundleOpened;

  public static void OpenFileBundle(IFileTreeLeafNode? fileTreeLeafNode,
                                    IAnnotatedFileBundle fileBundle)
    => OnFileBundleOpened?.Invoke(fileTreeLeafNode, fileBundle);
}