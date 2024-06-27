using fin.io.bundles;

using SceneGate.Ekona.Containers.Rom;

using uni.platforms;

using Yarhl.FileSystem;

namespace uni.games.nintendogs_labrador_and_friends;

public class NintendogsLabradorAndFriendsFileBundleGatherer
    : IAnnotatedFileBundleGatherer {
  public IEnumerable<IAnnotatedFileBundle> GatherFileBundles() {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
              "nintendogs_labrador_and_friends.nds",
              out var nintendogsRom)) {
        yield break;
      }

      using var game = NodeFactory.FromFile(nintendogsRom.FullPath);
      game.TransformWith<Binary2NitroRom>();

      var names = new List<string>();

      foreach (var node in Navigator.IterateNodes(game)) {
        names.Add(node.Name);
      }

      yield break;
    }
}