using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.asserts;

using Microsoft.Extensions.DependencyInjection;

using ServiceScan.SourceGenerator;

using uni.config;

namespace uni.games;

public static partial class ExtractorUtil {
  public const string CACHE = "cache";
  public const string PREREQS = "prereqs";
  public const string EXTRACTED = "extracted";

  public static ISystemDirectory GetOrCreateRomDirectory(
      ReadOnlySpan<char> romName)
    => DirectoryConstants.ROMS_DIRECTORY.GetOrCreateSubdir(romName);

  public static void GetOrCreateRomDirectoriesWithPrereqsAndCache(
      ReadOnlySpan<char> romName,
      out ISystemDirectory prereqsDir,
      out ISystemDirectory cacheDir,
      out ISystemDirectory extractedDir) {
    var romDir = GetOrCreateRomDirectory(romName);
    prereqsDir = romDir.GetOrCreateSubdir(PREREQS);
    cacheDir = romDir.GetOrCreateSubdir(CACHE);
    extractedDir = romDir.GetOrCreateSubdir(EXTRACTED);
  }

  public static void GetOrCreateRomDirectoriesWithPrereqs(
      ReadOnlySpan<char> romName,
      out ISystemDirectory prereqsDir,
      out ISystemDirectory extractedDir) {
    var romDir = GetOrCreateRomDirectory(romName);
    prereqsDir = romDir.GetOrCreateSubdir(PREREQS);
    extractedDir = romDir.GetOrCreateSubdir(EXTRACTED);
  }

  public static void GetOrCreateRomDirectoriesWithCache(
      ReadOnlySpan<char> romName,
      out ISystemDirectory cacheDir,
      out ISystemDirectory extractedDir) {
    var romDir = GetOrCreateRomDirectory(romName);
    cacheDir = romDir.GetOrCreateSubdir(CACHE);
    extractedDir = romDir.GetOrCreateSubdir(EXTRACTED);
  }


  public static ISystemDirectory GetOrCreateExtractedDirectory(
      IReadOnlyTreeFile romFile)
    => GetOrCreateExtractedDirectory(romFile.GetRomName());

  public static ISystemDirectory GetOrCreateExtractedDirectory(
      ReadOnlySpan<char> romName)
    => GetOrCreateRomDirectory(romName).GetOrCreateSubdir(EXTRACTED);


  public static IFileHierarchy GetFileHierarchy(
      ReadOnlySpan<char> romName,
      ISystemDirectory directory) {
    var romDir = GetOrCreateRomDirectory(romName);
    var cacheDir = romDir.GetOrCreateSubdir(CACHE);

    var cacheFile
        = new FinFile(Path.Join(cacheDir.FullPath, "hierarchy.cache"));

    return FileHierarchy.From(romName.ToString(), directory, cacheFile);
  }

  public static bool HasBeenExtracted(string gameName)
    => !HasNotBeenExtractedYet(gameName, out _);

  public static bool HasNotBeenExtractedYet(
      IReadOnlyTreeFile romFile,
      out ISystemDirectory extractedDir)
    => HasNotBeenExtractedYet(romFile.GetRomName(), out extractedDir);

  public static bool HasNotBeenExtractedYet(
      ReadOnlySpan<char> romName,
      out ISystemDirectory extractedDir) {
    extractedDir = GetOrCreateExtractedDirectory(romName);
    return extractedDir.IsEmpty;
  }


  public static ISystemDirectory GetOutputDirectoryForFileBundle(
      IFileBundle annotatedFileBundle) {
    Asserts.True(annotatedFileBundle.MainFile.HasRoot(out var gameName, out var localPath));
    var gameAndLocalPath = Path.Join(gameName, localPath);
    return new FinFile(Path.Join(
                           DirectoryConstants.OUT_DIRECTORY.FullPath,
                           gameAndLocalPath))
        .AssertGetParent();
  }

  [GenerateServiceRegistrations(AssignableTo
                                    = typeof(INamedFileBundleGatherer),
                                Lifetime = ServiceLifetime.Transient)]
  public static partial IServiceCollection AddGatherers(
      this IServiceCollection services);

  public static IEnumerable<INamedFileBundleGatherer> GetAllExtractors() {
    var gathererCollection = new ServiceCollection();
    gathererCollection.AddGatherers();

    using var gathererProvider = gathererCollection.BuildServiceProvider();
    var gatherers = gathererProvider.GetServices<INamedFileBundleGatherer>()
                                    .OrderBy(g => g.Name);

    return gatherers;
  }

  public static IEnumerable<(INamedFileBundleGatherer gatherer, bool
          stillNeedsToBeConfigured)>
      GetExtractorsWhichNeedConfiguration() {
    var configuredExtractableGames
        = Config.Instance.Extractor.GamesToExtract?.Keys.ToHashSet() ?? [];

    return GetAllExtractors()
        .Select(g => {
          var needsConfiguration = g is {
              IsAvailable: true, IsListed: true
          };

          return (
              g,
              needsConfiguration &&
              !configuredExtractableGames.Contains(g.Name));
        });
  }
}

static file class ExtractorUtilExtensions {
  public static ReadOnlySpan<char> GetRomName(this IReadOnlyTreeFile romFile)
    => romFile.NameWithoutExtension;
}