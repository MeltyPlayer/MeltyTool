using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.desktop;

using visceral.api;

namespace uni.games.dead_space_3;

public class DeadSpace3FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    ISystemDirectory deadSpace3Dir;
    if (!(SteamUtils.TryGetGameDirectory("Dead Space 3", out deadSpace3Dir) ||
          EaUtils.TryGetGameDirectory("Dead Space 3", out deadSpace3Dir))) {
      return;
    }

    ExtractorUtil.GetOrCreateRomDirectoriesWithPrereqs(
        "dead_space_3",
        out var prereqsDir,
        out var extractedDir);
    if (extractedDir.IsEmpty) {
      var bighExtractor = new BighExtractor();
      foreach (var vivFile in
               deadSpace3Dir.GetFilesWithFileType(".viv", true)) {
        var filelistFile = prereqsDir.AssertGetExistingFile(
            $"{vivFile.NameWithoutExtension}.filelist");

        bighExtractor.Extract(vivFile, filelistFile, extractedDir);
      }
    }
  }
}