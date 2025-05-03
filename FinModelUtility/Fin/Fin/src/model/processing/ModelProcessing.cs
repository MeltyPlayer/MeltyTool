using System.Collections.Generic;

using fin.io;
using fin.model.io;
using fin.model.io.importers;

namespace fin.model.processing;

public static class ModelProcessing {
  public static IModel ImportAndProcess<T>(this IModelImporter<T> modelImporter,
                                           T fileBundle)
      where T : IModelFileBundle {
    var model = modelImporter.Import(fileBundle);
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