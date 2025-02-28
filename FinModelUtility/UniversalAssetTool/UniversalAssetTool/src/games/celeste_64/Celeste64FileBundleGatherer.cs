using fin.audio.io.importers.midi;
using fin.common;
using fin.io.bundles;
using fin.model.io.importers.assimp;
using fin.util.progress;

namespace uni.games.celeste_64;

public class Celeste64FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join("celeste_64", ExtractorUtil.PREREQS),
            out var celeste64Dir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("celeste_64", celeste64Dir);

    foreach (var glbFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".glb")) {
      organizer.Add(new AssimpModelFileBundle {
          MainFile = glbFile
      }.Annotate(glbFile));
    }
  }
}