using fin.io;
using fin.io.bundles;

using uni.platforms;
using uni.util.io;

using xmod.api;

namespace uni.games.midnight_club_2 {
  public class MidnightClub2FileBundleGatherer
      : IAnnotatedFileBundleGatherer {
    public IEnumerable<IAnnotatedFileBundle> GatherFileBundles() {
      if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
              "midnight_club_2",
              out var midnightClub2Directory)) {
        return Enumerable.Empty<IAnnotatedFileBundle>();
      }

      var fileHierarchy =
          FileHierarchy.From("midnight_club_2", midnightClub2Directory);

      var textureDirectory =
          fileHierarchy.Root.AssertGetExistingSubdir("texture_x");

      return new FileHierarchyAssetBundleSeparator(
          fileHierarchy,
          subdir
              => subdir.FilesWithExtension(".xmod")
                       .Select(file => new XmodModelFileBundle {
                           GameName = "midnight_club_2",
                           XmodFile = file,
                           TextureDirectory = textureDirectory,
                       }.Annotate(file))
                       .Concat<IAnnotatedFileBundle>(
                           subdir.FilesWithExtension(".ped")
                                 .Select(
                                     file => new PedModelFileBundle {
                                         PedFile = file,
                                     }.Annotate(file)))).GatherFileBundles();
    }
  }
}