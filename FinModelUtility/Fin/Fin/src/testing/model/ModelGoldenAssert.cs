using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.io;
using fin.model.io;
using fin.model.io.importers;

namespace fin.testing.model;

public static class ModelGoldenAssert {
  public static IEnumerable<TModelBundle> GetGoldenModelBundles<TModelBundle>(
      ISystemDirectory rootGoldenDirectory,
      Func<IFileHierarchyDirectory, TModelBundle>
          gatherModelBundleFromInputDirectory)
      where TModelBundle : IModelFileBundle {
    foreach (var goldenSubdir in
             GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)) {
      var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
      var modelBundle = gatherModelBundleFromInputDirectory(inputDirectory);

      yield return modelBundle;
    }
  }

  /// <summary>
  ///   Asserts model goldens. Assumes that directories will be stored as the following:
  ///
  ///   - {goldenDirectory}
  ///     - {goldenName1}
  ///       - input
  ///         - {raw golden files}
  ///       - output
  ///         - {exported files}
  ///     - {goldenName2}
  ///       ... 
  /// </summary>
  public static void AssertExportGoldens<TModelBundle>(
      ISystemDirectory rootGoldenDirectory,
      IModelImporter<TModelBundle> modelImporter,
      Func<IFileHierarchyDirectory, TModelBundle>
          gatherModelBundleFromInputDirectory)
      where TModelBundle : IModelFileBundle {
    foreach (var goldenSubdir in
             GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)) {
      ModelGoldenAssert.AssertGolden(goldenSubdir,
                                     modelImporter,
                                     gatherModelBundleFromInputDirectory);
    }
  }

  private static string[] EXTENSIONS = [".glb"];

  public static void AssertGolden<TModelBundle>(
      IFileHierarchyDirectory goldenSubdir,
      IModelImporter<TModelBundle> modelImporter,
      Func<IFileHierarchyDirectory, TModelBundle>
          gatherModelBundleFromInputDirectory)
      where TModelBundle : IModelFileBundle {
    var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
    var modelBundle = gatherModelBundleFromInputDirectory(inputDirectory);

    var outputDirectory = goldenSubdir.AssertGetExistingSubdir("output");
    var hasGoldenExport =
        outputDirectory.GetExistingFiles()
                       .Any(file => EXTENSIONS.Contains(file.FileType));

    GoldenAssert.RunInTestDirectory(
        goldenSubdir,
        tmpDirectory => {
          var targetDirectory =
              hasGoldenExport ? tmpDirectory : outputDirectory.Impl;

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

          if (hasGoldenExport) {
            GoldenAssert.AssertFilesInDirectoriesAreIdentical(
                tmpDirectory,
                outputDirectory.Impl);
          }
        });
  }
}