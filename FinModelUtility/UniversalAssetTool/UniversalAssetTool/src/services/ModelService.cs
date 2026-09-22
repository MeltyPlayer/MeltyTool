using fin.io.bundles;
using fin.io.web;
using fin.model;
using fin.model.io;
using fin.model.processing;
using fin.services;
using fin.util.types;

using uni.api;
using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

[IocCandiate]
public static class ModelService {
  static ModelService() {
    FileBundleService.OnFileBundleOpened
        += (fileTreeLeafNode, fileBundle) => {
          LoadingStatusService.IsLoading = true;

          if (fileBundle is IModelFileBundle modelFileBundle) {
            try {
              var model = modelFileBundle.ImportAndProcess();
              OpenModel(fileTreeLeafNode, model);
            } catch (Exception e) {
              FailToOpenModel(fileBundle, e);
            }
          }

          LoadingStatusService.IsLoading = false;
        };
  }

  public static event Action<IFileTreeLeafNode?, IModel>?
      OnModelSuccessfullyOpened;

  public static event Action<IFileBundle?, Exception>? OnModelFailedToOpen;

  public static void OpenModel(
      IFileTreeLeafNode? fileTreeLeafNode,
      IModel model)
    => OnModelSuccessfullyOpened?.Invoke(fileTreeLeafNode, model);

  public static void FailToOpenModel(
      IFileBundle fileBundle,
      Exception exception)
    => OnModelFailedToOpen?.Invoke(fileBundle, exception);
}