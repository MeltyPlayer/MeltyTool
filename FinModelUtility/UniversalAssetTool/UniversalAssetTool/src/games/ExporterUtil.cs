using Assimp;

using grezzo.api;

using fin.importers;
using fin.model.io.exporters.assimp;
using fin.io;
using fin.io.bundles;
using fin.log;
using fin.model;
using fin.model.io;
using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.model.io.importers;
using fin.model.processing;
using fin.util.asserts;
using fin.util.linq;
using fin.util.progress;

using uni.config;
using uni.model;
using uni.msg;
using uni.thirdparty;

namespace uni.games;

public static class ExporterUtil {
  static ExporterUtil() {
    logger_ = Logging.Create("exportor");
  }

  private static readonly ILogger logger_;

  public static bool CheckIfFilesAlreadyExist(
      IEnumerable<ISystemFile> outputFiles,
      out IReadOnlyList<ISystemFile> existingOutputFiles) {
    existingOutputFiles =
        outputFiles.Where(file => file.Exists).ToArray();
    return existingOutputFiles.Count > 0;
  }

  public static bool CheckIfModelFileBundlesAlreadyExported(
      IEnumerable<IFileBundle> modelFileBundles,
      IReadOnlySet<ExportedFormat> formats,
      out IReadOnlyList<IFileBundle> existingModelFileBundles) {
    existingModelFileBundles =
        modelFileBundles
            .Where(mfb => CheckIfModelFileBundleAlreadyExported(
                       mfb,
                       formats))
            .ToArray();
    return existingModelFileBundles.Count > 0;
  }

  public static bool CheckIfModelFileBundleAlreadyExported(
      IFileBundle annotatedModelFileBundle,
      IEnumerable<ExportedFormat> formats) {
    // TODO: Clean this up!!
    var bundle = annotatedModelFileBundle;
    var mainFile = bundle.MainFile;

    var parentOutputDirectory =
        ExtractorUtil.GetOutputDirectoryForFileBundle(
            annotatedModelFileBundle);
    var outputDirectory = new FinDirectory(
        Path.Join(parentOutputDirectory.FullPath,
                  mainFile.NameWithoutExtension));

    if (outputDirectory.Exists) {
      return formats
             .AsFileExtensions()
             .All(extension => outputDirectory
                               .GetExistingFiles()
                               .Where(file => extension == file.FileType)
                               .Any(file => file.NameWithoutExtension
                                                .SequenceEqual(
                                                    mainFile
                                                        .NameWithoutExtension)));
    }

    return false;
  }

  public static ExportedFormat[] SupportedExportFormats { get; } = [
      ExportedFormat.DAE,
      ExportedFormat.FBX,
      ExportedFormat.GLB,
      ExportedFormat.GLTF,
      ExportedFormat.OBJ,
  ];

  public static IEnumerable<string> AsFileExtensions(
      this IEnumerable<ExportedFormat> formats)
    => formats.Select(AsFileExtension);

  public static string GetName(this ExportedFormat format)
    => format switch {
        ExportedFormat.DAE => "COLLADA",
        ExportedFormat.FBX => "FilmBox",
        ExportedFormat.GLB => "GL Transmission Format (binary)",
        ExportedFormat.GLTF => "GL Transmission Format (text)",
        ExportedFormat.OBJ => "Wavefront",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };

  public static string AsFileExtension(this ExportedFormat format)
    => format switch {
        ExportedFormat.DAE => ".dae",
        ExportedFormat.FBX => ".fbx",
        ExportedFormat.GLB => ".glb",
        ExportedFormat.GLTF => ".gltf",
        ExportedFormat.OBJ => ".obj",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };

  public static string AsPattern(this ExportedFormat format)
    => $"*{format.AsFileExtension()}";

  public enum ExporterPromptChoice {
    CANCEL,
    SKIP_EXISTING,
    OVERWRITE_EXISTING,
  }

  public static List<IFileBundle> GatherFileBundles(
      this IAnnotatedFileBundleGatherer gatherer) {
    var organizer = new FileBundleListOrganizer();
    var progress = new PercentageProgress();
    gatherer.GatherFileBundles(organizer, progress);
    return organizer.List;
  }

  public static void ExportAllForCli<T>(
      IAnnotatedFileBundleGatherer gatherer,
      IModelImporter<T> reader)
      where T : IModelFileBundle
    => ExportAllForCli_(
        gatherer.GatherFileBundles(),
        reader,
        Config.Instance.Exporter.General.ExportedFormats,
        false);

  public static void ExportAllOfTypeForCli<TSubType>(
      IAnnotatedFileBundleGatherer gatherer,
      IModelImporter<TSubType> reader)
      where TSubType : IModelFileBundle
    => ExportAllForCli_(
        gatherer.GatherFileBundles().OfType<TSubType>(),
        reader,
        Config.Instance.Exporter.General.ExportedFormats,
        false);

  private static void ExportAllForCli_<T>(
      IEnumerable<IFileBundle> fileBundles,
      IModelImporter<T> reader,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFiles)
      where T : IModelFileBundle
    => ExportAllForCli_(
        fileBundles.OfType<T>(),
        reader,
        formats,
        overwriteExistingFiles);

  private static void ExportAllForCli_<T>(
      IEnumerable<T> modelFileBundles,
      IModelImporter<T> reader,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFiles)
      where T : IModelFileBundle {
    var bundlesArray = modelFileBundles.ToArray();
    Asserts.True(bundlesArray.Length > 0,
                 "Expected to find bundles for the current ROM. Does the file exist, and was it exported correctly?");

    foreach (var modelFileBundle in bundlesArray) {
      Export(modelFileBundle,
             reader,
             formats,
             overwriteExistingFiles);
    }
  }


  public static void ExportAll<T>(
      IEnumerable<IFileBundle> fileBundles,
      IModelImporter<T> reader,
      IProgress<(float, T?)> progress,
      CancellationTokenSource cancellationTokenSource,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFiles)
      where T : IModelFileBundle {
    var fileBundleArray = fileBundles.OfType<T>().ToArray();
    for (var i = 0; i < fileBundleArray.Length; ++i) {
      if (cancellationTokenSource.IsCancellationRequested) {
        break;
      }

      var modelFileBundle = fileBundleArray[i];
      progress.Report((i * 1f / fileBundleArray.Length, modelFileBundle));
      Export(modelFileBundle,
             reader,
             formats,
             overwriteExistingFiles);
    }

    progress.Report((1, default));
  }

  public static void Export<T>(T modelFileBundle,
                               IModelImporter<T> reader,
                               IReadOnlySet<ExportedFormat> formats,
                               bool overwriteExistingFile)
      where T : IModelFileBundle {
    Export(modelFileBundle,
           () => reader.ImportAndProcess(modelFileBundle),
           formats,
           overwriteExistingFile);

    if (modelFileBundle is CmbModelFileBundle cmbModelFileBundle &&
        reader is IModelImporter<CmbModelFileBundle> cmbReader) {
      ExportAnimationsForCmb_(cmbModelFileBundle,
                              cmbReader,
                              formats,
                              overwriteExistingFile);
    }
  }

  private static void ExportAnimationsForCmb_(
      CmbModelFileBundle modelFileBundle,
      IModelImporter<CmbModelFileBundle> reader,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFile) {
    if (modelFileBundle.CsabFiles == null || modelFileBundle.CsabFiles.Count == 0) {
      return;
    }

    if (!formats.Contains(ExportedFormat.FBX)) {
      return;
    }

    var mainFile = modelFileBundle.MainFile;
    var parentOutputDirectory =
        ExtractorUtil.GetOutputDirectoryForFileBundle(modelFileBundle);
    var outputDirectory = new FinDirectory(
        Path.Join(parentOutputDirectory.FullPath,
                  mainFile.NameWithoutExtension));

    var fbxFormat = AssimpUtil.GetExportFormatFromExtension(".fbx");

    foreach (var csabFile in modelFileBundle.CsabFiles) {
      var animationName = csabFile.NameWithoutExtension.ToString();
      var animationOutput = new FinFile(
          Path.Join(outputDirectory.FullPath,
                    $"{animationName}.foo"));

      var expectedOutput = animationOutput.CloneWithFileType(".fbx");
      if (!overwriteExistingFile && expectedOutput.Exists) {
        continue;
      }

      var singleAnimationBundle = new CmbModelFileBundle(
          modelFileBundle.CmbFile,
          [csabFile],
          modelFileBundle.CtxbFiles,
          modelFileBundle.ShpaFiles);

      var animationModel = reader.ImportAndProcess(singleAnimationBundle);
      var animationScale =
          new ScaleSource(
                  Config.Instance.Exporter.General.ExportedModelScaleSource)
              .GetScale(animationModel);

      new AssimpIndirectModelExporter {
          AnimationOnly = true,
      }.ExportFormats(
          new ModelExporterParams {
              OutputFile = animationOutput,
              Model = animationModel,
              Scale = animationScale,
          },
          [fbxFormat],
          false);
    }
  }

  public static void Export<T>(T threeDFileBundle,
                               Func<IModel> loaderHandler,
                               IReadOnlySet<ExportedFormat> formats,
                               bool overwriteExistingFile)
      where T : I3dFileBundle {
    var mainFile = Asserts.CastNonnull(threeDFileBundle.MainFile);

    var parentOutputDirectory =
        ExtractorUtil.GetOutputDirectoryForFileBundle(threeDFileBundle);
    var outputDirectory = new FinDirectory(
        Path.Join(parentOutputDirectory.FullPath,
                  mainFile.NameWithoutExtension));

    Export(threeDFileBundle,
           loaderHandler,
           outputDirectory,
           formats,
           overwriteExistingFile);
  }

  public static void Export<T>(T threeDFileBundle,
                               Func<IModel> loaderHandler,
                               ISystemDirectory outputDirectory,
                               IReadOnlySet<ExportedFormat> formats,
                               bool overwriteExistingFile,
                               string? overrideName = null)
      where T : I3dFileBundle
    => Export(threeDFileBundle,
              loaderHandler,
              outputDirectory,
              FilterFormatsForConfiguredExporter_(formats.AsFileExtensions()
                                                         .Select(AssimpUtil.GetExportFormatFromExtension)
                                                         .ToArray()),
              overwriteExistingFile,
              overrideName);

  public static void Export<T>(
      T threeDFileBundle,
      Func<IModel> loaderHandler,
      ISystemDirectory outputDirectory,
      IReadOnlyList<ExportFormatDescription> formats,
      bool overwriteExistingFile,
      string? overrideName = null)
      where T : I3dFileBundle {
    var mainFile = Asserts.CastNonnull(threeDFileBundle.MainFile);
    var name = (overrideName ?? mainFile.NameWithoutExtension).ToString();

    if (threeDFileBundle.UseLowLevelExporter) {
      formats = [AssimpUtil.GetExportFormatFromExtension(".gltf")];
    }

    formats = FilterFormatsForConfiguredExporter_(formats);

    var targetFiles = formats.Select(format => new FinFile(
                                         Path.Join(outputDirectory.FullPath,
                                                   $"{name}.{format.FileExtension}")));
    if (!overwriteExistingFile &&
        targetFiles.All(targetFile => targetFile.Exists)) {
      MessageUtil.LogAlreadyProcessed(logger_, mainFile);
      return;
    }

    outputDirectory.Create();
    MessageUtil.LogExporting(logger_, mainFile);

    try {
      var model = loaderHandler();

      new AssimpIndirectModelExporter {
          LowLevel = threeDFileBundle.UseLowLevelExporter,
          ForceGarbageCollection = threeDFileBundle.ForceGarbageCollection,
      }.ExportFormats(new ModelExporterParams {
                          OutputFile = new FinFile(
                              Path.Join(outputDirectory.FullPath,
                                        $"{name}.foo")),
                          Model = model,
                          Scale = new ScaleSource(
                                  Config.Instance.Exporter.General
                                        .ExportedModelScaleSource)
                              .GetScale(model)
                      },
                      formats,
                      Config.Instance.Exporter.General.ExportAllTextures);

      if (Config.Instance.Exporter.ThirdParty
                .ExportBoneScaleAnimationsSeparately) {
        new BoneScaleAnimationExporter().Export(
            new FinFile(Path.Join(outputDirectory.FullPath,
                                  $"{name}_bone_scale_animations.lua")),
            model);
      }
    } catch (Exception e) {
      logger_.LogError(e.ToString());
    }

    logger_.LogInformation(" ");
  }

  private static IReadOnlyList<ExportFormatDescription> FilterFormatsForConfiguredExporter_(
      IReadOnlyList<ExportFormatDescription> formats) {
    if (!BlenderHeadlessFbxExporter.IsConfigured()) {
      return formats;
    }

    var hasFbx = formats.Any(format => format.FileExtension.Equals("fbx",
                                                                   StringComparison.OrdinalIgnoreCase));
    if (!hasFbx) {
      return formats;
    }

    return formats.Where(format => !format.FileExtension.Equals("glb",
                                                                StringComparison.OrdinalIgnoreCase))
                  .ToArray();
  }
}
