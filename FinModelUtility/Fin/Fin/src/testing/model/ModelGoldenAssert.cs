using System;
using System.IO;

using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.io;
using fin.model.io;
using fin.model.io.importers;

namespace fin.testing.model;

public static class ModelGoldenAssert {
  private static string[] EXTENSIONS = [".glb"];

  public static void AssertGolden<TModelBundle>(
      IFileHierarchyDirectory goldenSubdir,
      IModelImporter<TModelBundle> modelImporter,
      Func<IFileHierarchyDirectory, TModelBundle>
          gatherModelBundleFromInputDirectory)
      where TModelBundle : IModelFileBundle {
    GoldenAssert.AssertGoldenFiles(
        goldenSubdir,
        (inputDirectory, targetDirectory) => {
          var modelBundle = gatherModelBundleFromInputDirectory(inputDirectory);
          var model = modelImporter.Import(modelBundle);
          new AssimpIndirectModelExporter() {
              LowLevel = modelBundle.UseLowLevelExporter,
              ForceGarbageCollection = modelBundle.ForceGarbageCollection,
          }.ExportExtensions(
              new ModelExporterParams {
                  Model = model,
                  OutputFile =
                      new FinFile(Path.Combine(targetDirectory.FullPath,
                                               $"{modelBundle.MainFile.NameWithoutExtension}.foo")),
              },
              EXTENSIONS,
              true);
        });
  }
}