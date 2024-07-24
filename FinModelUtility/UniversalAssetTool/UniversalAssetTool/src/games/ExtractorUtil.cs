using fin.io;
using fin.io.bundles;

using uni.platforms;

namespace uni.games;

public static class ExtractorUtil {
  public const string PREREQS = "prereqs";
  public const string EXTRACTED = "extracted";

  public static ISystemDirectory GetOrCreateRomDirectory(
      IReadOnlyTreeFile romFile)
    => GetOrCreateRomDirectory(romFile.GetRomName());

  public static ISystemDirectory GetOrCreateRomDirectory(
      string romName)
    => DirectoryConstants.ROMS_DIRECTORY.GetOrCreateSubdir(romName);


  public static void GetOrCreateRomDirectories(
      IReadOnlyTreeFile romFile,
      out ISystemDirectory prereqsDir,
      out ISystemDirectory extractedDir)
    => GetOrCreateRomDirectories(romFile.GetRomName(),
                                 out prereqsDir,
                                 out extractedDir);

  public static void GetOrCreateRomDirectories(
      string romName,
      out ISystemDirectory prereqsDir,
      out ISystemDirectory extractedDir) {
    var romDir = GetOrCreateRomDirectory(romName);
    prereqsDir = romDir.GetOrCreateSubdir(PREREQS);
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
    var prereqsDir = romDir.GetOrCreateSubdir(PREREQS);

    var cacheFile
        = new FinFile(Path.Join(prereqsDir.FullPath, "hierarchy.cache"));

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