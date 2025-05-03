using fin.io.web;
using fin.model;
using fin.model.io;
using fin.model.processing;
using fin.services;

using uni.api;
using uni.ui.winforms.common.fileTreeView;

namespace uni;

public static class ModelService {
  static ModelService() {
    FileBundleService.OnFileBundleOpened
        += (fileTreeLeafNode, fileBundle) => {
          if (fileBundle.FileBundle is IModelFileBundle modelFileBundle) {
            try {
              var model
                  = new GlobalModelImporter().ImportAndProcess(modelFileBundle);
              OpenModel(fileTreeLeafNode, model);
            } catch (Exception e) {
              ExceptionService.HandleException(
                  e,
                  new LoadFileBundleExceptionContext(fileBundle));
            }
          }
        };
  }

  public static event Action<IFileTreeLeafNode?, IModel>? OnModelOpened;

  public static void OpenModel(IFileTreeLeafNode? fileTreeLeafNode,
                               IModel model)
    => OnModelOpened?.Invoke(fileTreeLeafNode, model);
}