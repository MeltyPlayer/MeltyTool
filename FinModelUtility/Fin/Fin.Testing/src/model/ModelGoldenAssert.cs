using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.io;
using fin.math.rotations;
using fin.model.io;
using fin.model.io.importers;
using fin.model.processing;
using fin.ui.rendering.gl;


namespace fin.testing.model;

public static class ModelGoldenAssert {
  private static string[] EXTENSIONS = [".glb"];

  public static async Task AssertGolden<TModelBundle>(
      IFileHierarchyDirectory goldenSubdir,
      IModelImporter<TModelBundle> modelImporter,
      Func<IFileHierarchyDirectory, TModelBundle>
          gatherModelBundleFromInputDirectory)
      where TModelBundle : IModelFileBundle {
    QuaternionUtil.UseSlowButConsistentSlerp();
    OpenGlVersionService.Init(false, 4, 5);

    await GoldenAssert.AssertGoldenFiles(
        goldenSubdir,
        (inputDirectory, targetDirectory) => {
          var modelBundle = gatherModelBundleFromInputDirectory(inputDirectory);

          var model = modelImporter.ImportAndProcess(modelBundle);
          
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