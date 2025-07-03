using uni.ui.winforms.common.fileTreeView;

namespace uni;

public static class SelectedFileTreeDirectoryService {
  public static event Action<IFileTreeParentNode>? OnFileTreeDirectorySelected;

  public static void SelectFileTreeDirectory(
      IFileTreeParentNode fileTreeParentNode)
    => OnFileTreeDirectorySelected?.Invoke(fileTreeParentNode);
}