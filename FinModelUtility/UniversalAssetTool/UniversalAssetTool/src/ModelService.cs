using fin.model;
using fin.model.io;
using fin.scene;

using uni.api;

namespace uni {
  public static class ModelService {
    static ModelService() {
      FileBundleService.OnFileBundleOpened
          += fileBundle => {
               if (fileBundle is IModelFileBundle modelFileBundle) {
                 var model = new GlobalModelImporter().Import(modelFileBundle);
                 OpenModel(model);
               }
             };
    }

    public static event Action<IModel> OnModelOpened;

    public static void OpenModel(IModel model)
      => OnModelOpened?.Invoke(model);
  }
}