using fin.io.bundles;

using uni.ui.winforms.common.fileTreeView;

namespace uni {
  public static class FileBundleService {
    static FileBundleService() {
      FileTreeLeafNodeService.OnFileTreeLeafNodeOpened
          += fileTreeLeafNode => {
               var fileBundle = fileTreeLeafNode.File.FileBundle;
               OpenFileBundle(fileTreeLeafNode, fileBundle);
             };
    }

    public static event Action<IFileTreeLeafNode?, IFileBundle>
        OnFileBundleOpened;

    public static void OpenFileBundle(IFileTreeLeafNode? fileTreeLeafNode,
                                      IFileBundle fileBundle)
      => OnFileBundleOpened?.Invoke(fileTreeLeafNode, fileBundle);
  }
}