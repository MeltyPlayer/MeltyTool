using System.Collections.Generic;

using fin.io;
using fin.model.io;

namespace fin.model.processing;

public static class ModelProcessing {
  public static IModel ImportAndProcess<T>(this T fileBundle)
      where T : IModelFileBundle {
    var model = fileBundle.Import();
    ProcessAfterLoad(model);
    return model;
  }

  public static IModel ImportAndProcess(
      this IModelImporterPlugin modelImporterPlugin,
      IEnumerable<IReadOnlyTreeFile> files,
      float frameRate = 30) {
    var model = modelImporterPlugin.Import(files, frameRate);
    ProcessAfterLoad(model);
    return model;
  }

  public static void ProcessAfterLoad(IModel model) {
    NameFixing.FixNames(model);
  }

  public static void BeforeExport(IModel model) {
    //TextureTransformBaking.TryToBakeTextureTransforms(model);
  }
}