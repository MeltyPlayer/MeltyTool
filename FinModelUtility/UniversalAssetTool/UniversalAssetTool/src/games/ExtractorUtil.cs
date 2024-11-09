using fin.io;
using fin.io.bundles;

using uni.platforms;

namespace uni.games;

public static class ExtractorUtil {
  public const string CACHE = "cache";
  public const string PREREQS = "prereqs";
  public const string EXTRACTED = "extracted";

  public static ISystemDirectory GetOrCreateRomDirectory(
      string romName)
    => DirectoryConstants.ROMS_DIRECTORY.GetOrCreateSubdir(romName);


  public static void GetOrCreateRomDirectoriesWithPrereqs(
      string romName,
      out ISystemDirectory prereqsDir,
      out ISystemDirectory extractedDir) {
    var romDir = GetOrCreateRomDirectory(romName);
    prereqsDir = romDir.GetOrCreateSubdir(PREREQS);
    extractedDir = romDir.GetOrCreateSubdir(EXTRACTED);
  }

  public static void GetOrCreateRomDirectoriesWithCache(
      string romName,
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
      string romName)
    => GetOrCreateRomDirectory(romName).GetOrCreateSubdir(EXTRACTED);


  public static IFileHierarchy GetFileHierarchy(
      string romName,
      ISystemDirectory directory) {
    var romDir = GetOrCreateRomDirectory(romName);
    var cacheDir = romDir.GetOrCreateSubdir(CACHE);

    var cacheFile
        = new FinFile(Path.Join(cacheDir.FullPath, "hierarchy.cache"));

    return FileHierarchy.From(romName, directory, cacheFile);
  }


  public static bool HasNotBeenExtractedYet(
      IReadOnlyTreeFile romFile,
      out ISystemDirectory extractedDir)
    => HasNotBeenExtractedYet(romFile.GetRomName(), out extractedDir);

  public static bool HasNotBeenExtractedYet(
      string romName,
      out ISystemDirectory extractedDir) {
    extractedDir = GetOrCreateExtractedDirectory(romName);
    return extractedDir.IsEmpty;
  }


  public static ISystemDirectory GetOutputDirectoryForFileBundle(
      IAnnotatedFileBundle annotatedFileBundle)
    => new FinFile(Path.Join(
                       DirectoryConstants.OUT_DIRECTORY.FullPath,
                       annotatedFileBundle.GameAndLocalPath)).AssertGetParent();
}

static file class ExtractorUtilExtensions {
  public static string GetRomName(this IReadOnlyTreeFile romFile)
    => romFile.NameWithoutExtension;
}