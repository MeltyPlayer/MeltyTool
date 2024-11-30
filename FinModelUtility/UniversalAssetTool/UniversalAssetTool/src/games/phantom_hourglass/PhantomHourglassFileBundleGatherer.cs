using fin.io.bundles;
using fin.util.progress;

using SceneGate.Ekona.Containers.Rom;

using uni.platforms;

using Yarhl.FileSystem;

namespace uni.games.phantom_hourglass;

public class PhantomHourglassFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "phantom_hourglass.nds",
            out var phantomHourglassRom)) {
      return;
    }

    using var game = NodeFactory.FromFile(phantomHourglassRom.FullPath);
    game.TransformWith<Binary2NitroRom>();

    var names = new List<string>();

    foreach (var node in Navigator.IterateNodes(game)) {
      names.Add(node.Name);
    }
  }
}