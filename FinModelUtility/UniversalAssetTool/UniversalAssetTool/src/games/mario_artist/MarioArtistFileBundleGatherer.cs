using Celeste64.api;

using fin.io;
using fin.io.bundles;
using fin.model.io.importers.gltf;
using fin.util.progress;

using fmod.api;

using marioartist.schema;


namespace uni.games.mario_artist;

public class MarioArtistFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "mario_artist";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;

    foreach (var tstltFile in root.FilesWithExtensionRecursive(".tstlt")) {
      organizer.Add(new TstltModelFileBundle(tstltFile).Annotate(tstltFile));
    }
  }
}