using fin.model;
using fin.model.io;
using fin.scene;

using uni.api;
using uni.ui.winforms.common.fileTreeView;

namespace uni {
  public static class ModelService {
    static ModelService() {
      FileTreeLeafNodeService.OnFileTreeLeafNodeOpened
          += fileTreeLeafNode => {
               var fileBundle = fileTreeLeafNode.File.FileBundle;
               if (fileBundle is IModelFileBundle modelFileBundle) {
                 var model = new GlobalModelImporter().Import(modelFileBundle);
                 OpenModel(fileTreeLeafNode, model);
               }
             };
    }

    public static event Action<IFileTreeLeafNode?, IModel> OnModelOpened;

    public static void OpenModel(IFileTreeLeafNode? fileTreeLeafNode,
                                 IModel model)
      => OnModelOpened?.Invoke(fileTreeLeafNode, model);
  }
}