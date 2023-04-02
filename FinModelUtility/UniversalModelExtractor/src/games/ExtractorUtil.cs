using fin.exporter;
using fin.exporter.assimp.indirect;
using fin.io;
using fin.io.bundles;
using fin.log;
using fin.model;
using fin.util.asserts;

using uni.config;
using uni.model;
using uni.msg;
using uni.thirdparty;


namespace uni.games {
  public static class ExtractorUtil {
    static ExtractorUtil() {
      ExtractorUtil.logger_ = Logging.Create("extractor");
    }

    private static readonly ILogger logger_;

    public static void ExtractAll<T>(
        IFileBundleGatherer<T> gatherer,
        IModelLoader<T> loader)
        where T : IModelFileBundle {
      ExtractorUtil.ExtractAll(gatherer.GatherFileBundles(true), loader);
    }

    public static void ExtractAll<T>(
        IEnumerable<T> modelFileBundles,
        IModelLoader<T> loader)
        where T : IModelFileBundle {
      foreach (var modelFileBundle in modelFileBundles) {
        ExtractorUtil.Extract(modelFileBundle, loader);
      }
    }

    public static void ExtractAll<T>(
        IFileBundleGatherer<IFileBundle> gatherer,
        IModelLoader<T> loader)
        where T : IModelFileBundle {
      ExtractorUtil.ExtractAll(gatherer.GatherFileBundles(true), loader);
    }

    public static void ExtractAll<T>(
        IEnumerable<IFileBundle> fileBundles,
        IModelLoader<T> loader)
        where T : IModelFileBundle {
      foreach (var fileBundle in fileBundles) {
        if (fileBundle is T modelFileBundle) {
          ExtractorUtil.Extract(modelFileBundle, loader);
        }
      }
    }

    public static void Extract<T>(T modelFileBundle, IModelLoader<T> loader)
        where T : IModelFileBundle {
      ExtractorUtil.Extract(modelFileBundle,
                            () => loader.LoadModel(modelFileBundle));
    }

    public static void Extract<T>(T modelFileBundle, Func<IModel> loaderHandler)
        where T : IModelFileBundle {
      var mainFile = Asserts.CastNonnull(modelFileBundle.MainFile);

      var parentOutputDirectory =
          GameFileHierarchyUtil.GetOutputDirectoryForFile(mainFile);
      var outputDirectory = new FinDirectory(
          Path.Join(parentOutputDirectory.FullName,
                    mainFile.NameWithoutExtension));

      if (outputDirectory.Exists) {
        var existingOutputFile =
            outputDirectory.GetExistingFiles()
                           .Where(file => file.Extension is ".fbx" or ".glb"
                                      or ".gltf")
                           .Any(file => file.NameWithoutExtension ==
                                        mainFile.NameWithoutExtension);

        if (existingOutputFile) {
          MessageUtil.LogAlreadyProcessed(ExtractorUtil.logger_, mainFile);
          return;
        }
      }

      MessageUtil.LogExtracting(ExtractorUtil.logger_, mainFile);

      try {
        var model = loaderHandler();

        outputDirectory.Create();

        if (Config.Instance.ExportAllTextures) {
          foreach (var texture in model.MaterialManager.Textures) {
            texture.SaveInDirectory(outputDirectory);
          }
        }

        new AssimpIndirectExporter {
            LowLevel = modelFileBundle.UseLowLevelExporter,
            ForceGarbageCollection = modelFileBundle.ForceGarbageCollection,
        }.Export(new ExporterParams {
                     OutputFile = new FinFile(
                         Path.Join(outputDirectory.FullName,
                                   mainFile.NameWithoutExtension + ".foo")),
                     Model = model,
                     Scale = new ScaleSource(
                         Config.Instance.ExportedModelScaleSource).GetScale(
                         model,
                         modelFileBundle)
                 },
                 !modelFileBundle.UseLowLevelExporter
                     ? Config.Instance.ExportedFormats
                     : new[] { ".gltf" });

        if (Config.Instance.ThirdParty.ExportBoneScaleAnimationsSeparately) {
          new BoneScaleAnimationExporter().Export(
              new FinFile(Path.Join(outputDirectory.FullName,
                                    mainFile.NameWithoutExtension +
                                    "_bone_scale_animations.lua")),
              model);
        }
      } catch (Exception e) {
        ExtractorUtil.logger_.LogError(e.ToString());
      }

      ExtractorUtil.logger_.LogInformation(" ");
    }
  }
}