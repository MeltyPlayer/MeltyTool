using fin.common;
using fin.io.bundles;
using fin.util.progress;

using SceneGate.Ekona.Containers.Rom;

using Yarhl.FileSystem;

namespace uni.games.nintendogs_labrador_and_friends;

public class NintendogsLabradorAndFriendsFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
              "nintendogs_labrador_and_friends.nds",
              out var nintendogsRom)) {
        return;
      }

      using var game = NodeFactory.FromFile(nintendogsRom.FullPath);
      game.TransformWith<Binary2NitroRom>();

      var names = new List<string>();

      foreach (var node in Navigator.IterateNodes(game)) {
        names.Add(node.Name);
      }
    }
}