using Assimp;

using fin.importers;
using fin.model.io.exporters.assimp;
using fin.io;
using fin.io.bundles;
using fin.log;
using fin.model;
using fin.model.io;
using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.model.processing;
using fin.util.asserts;
using fin.util.progress;

using uni.config;
using uni.model;
using uni.msg;
using uni.thirdparty;

namespace uni.games;

using ExportResultTuple
    = (IFileBundle fileBundle, ExportResult result, Exception? exception);

public enum ExportResult {
  SKIPPED,
  ALREADY_EXISTS,
  FAILURE,
  SUCCESS,
}

public static class ExporterUtil {
  static ExporterUtil() {
    logger_ = Logging.Create("exporter");
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
    var name = GetExportName(annotatedModelFileBundle);
    var outputDirectory = GetOutputDirectory_(annotatedModelFileBundle);

    if (outputDirectory.Exists) {
      return formats
             .AsFileExtensions()
             .All(extension => outputDirectory
                               .GetExistingFiles()
                               .Where(file => extension == file.FileType)
                               .Any(file => file.NameWithoutExtension
                                                .SequenceEqual(name)));
    }

    return false;
  }

  /// <summary>
  ///   The name a bundle is exported under, which is usually its main file's
  ///   name. Bundles in a virtual directory that are only part of their main
  ///   file use their name in the file tree instead, so they don't collide.
  /// </summary>
  public static string GetExportName(IFileBundle fileBundle) {
    var mainFile = fileBundle.MainFile;
    var rawName = fileBundle.RawName;
    return fileBundle.VirtualDirectoryPath != null &&
           !rawName.SequenceEqual(mainFile.Name)
        ? rawName.ToString()
        : mainFile.NameWithoutExtension.ToString();
  }

  /// <summary>
  ///   Each bundle is exported into a folder of its own, named after it, so
  ///   bundles that share a parent folder don't overwrite each other's
  ///   textures.
  /// </summary>
  private static FinDirectory GetOutputDirectory_(IFileBundle fileBundle)
    => new FinDirectory(
        Path.Join(
            ExtractorUtil.GetOutputDirectoryForFileBundle(fileBundle).FullPath,
            GetExportName(fileBundle)));

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
      this IFileBundleGatherer gatherer) {
    var organizer = new FileBundleListOrganizer();
    var progress = new PercentageProgress();
    gatherer.GatherFileBundles(organizer, progress);
    return organizer.List;
  }

  public static void ExportAllForCli(
      IFileBundleGatherer gatherer)
    => ExportAllForCli_(
        gatherer.GatherFileBundles().OfType<IModelFileBundle>(),
        Config.Instance.Exporter.General.ExportedFormats,
        false);

  private static void ExportAllForCli_<T>(
      IEnumerable<T> modelFileBundles,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFiles)
      where T : IModelFileBundle {
    var bundlesArray = modelFileBundles.ToArray();
    Asserts.True(bundlesArray.Length > 0,
                 "Expected to find bundles for the current ROM. Does the file exist, and was it exported correctly?");

    foreach (var modelFileBundle in bundlesArray) {
      Export(modelFileBundle,
             formats,
             overwriteExistingFiles);
    }
  }


  public static ExportResultTuple[] ExportAll<T>(
      IEnumerable<IFileBundle> fileBundles,
      IProgress<(float, T?)> progress,
      CancellationTokenSource cancellationTokenSource,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFiles)
      where T : IModelFileBundle {
    var fileBundleArray = fileBundles.OfType<T>().ToArray();

    var results = new ExportResultTuple[fileBundleArray.Length];

    for (var i = 0; i < fileBundleArray.Length; ++i) {
      var modelFileBundle = fileBundleArray[i];

      if (cancellationTokenSource.IsCancellationRequested) {
        results[i] = (modelFileBundle, ExportResult.SKIPPED, null);
        continue;
      }

      progress.Report((i * 1f / fileBundleArray.Length, modelFileBundle));
      results[i] = Export(
          modelFileBundle,
          formats,
          overwriteExistingFiles);
    }

    progress.Report((1, default));

    return results;
  }

  public static ExportResultTuple Export<T>(
      T modelFileBundle,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFile)
      where T : IModelFileBundle
    => Export(modelFileBundle,
              () => modelFileBundle.ImportAndProcess(),
              formats,
              overwriteExistingFile);

  public static ExportResultTuple Export<T>(
      T threeDFileBundle,
      Func<IModel> loaderHandler,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFile)
      where T : I3dFileBundle {
    return Export(threeDFileBundle,
                  loaderHandler,
                  GetOutputDirectory_(threeDFileBundle),
                  formats,
                  overwriteExistingFile);
  }

  public static ExportResultTuple Export<T>(
      T threeDFileBundle,
      Func<IModel> loaderHandler,
      ISystemDirectory outputDirectory,
      IReadOnlySet<ExportedFormat> formats,
      bool overwriteExistingFile,
      string? overrideName = null)
      where T : I3dFileBundle
    => Export(threeDFileBundle,
              loaderHandler,
              outputDirectory,
              formats.AsFileExtensions()
                     .Select(AssimpUtil.GetExportFormatFromExtension)
                     .ToArray(),
              overwriteExistingFile,
              overrideName);

  public static ExportResultTuple Export<T>(
      T threeDFileBundle,
      Func<IModel> loaderHandler,
      ISystemDirectory outputDirectory,
      IReadOnlyList<ExportFormatDescription> formats,
      bool overwriteExistingFile,
      string? overrideName = null)
      where T : I3dFileBundle {
    var name = overrideName ?? GetExportName(threeDFileBundle);
    var displayPath = threeDFileBundle.DisplayFullPath.ToString();

    if (threeDFileBundle.UseLowLevelExporter) {
      formats = [AssimpUtil.GetExportFormatFromExtension(".gltf")];
    }

    var targetFiles = formats.Select(format => new FinFile(
                                         Path.Join(outputDirectory.FullPath,
                                                   $"{name}.{format.FileExtension}")));
    if (!overwriteExistingFile &&
        targetFiles.All(targetFile => targetFile.Exists)) {
      MessageUtil.LogAlreadyProcessed(logger_, displayPath);
      return (threeDFileBundle, ExportResult.ALREADY_EXISTS, null);
    }

    outputDirectory.Create();
    MessageUtil.LogExporting(logger_, displayPath);

    Exception? exception = null;

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
      exception = e;
      logger_.LogError(e.ToString());
    }

    logger_.LogInformation(" ");

    return (threeDFileBundle, 
            exception != null ? ExportResult.FAILURE : ExportResult.SUCCESS,
            exception);
  }
}