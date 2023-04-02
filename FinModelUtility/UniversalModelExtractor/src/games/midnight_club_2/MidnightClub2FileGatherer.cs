using fin.io;
using fin.io.bundles;
using fin.model;

using uni.platforms;
using uni.util.io;

using xmod.api;


namespace uni.games.midnight_club_2 {
  public class MidnightClub2FileGatherer
      : IFileBundleGatherer<IModelFileBundle> {
    public IEnumerable<IModelFileBundle> GatherFileBundles(
        bool assert) {
      var midnightClub2Directory =
          DirectoryConstants.ROMS_DIRECTORY.GetSubdir("midnight_club_2");
      if (!midnightClub2Directory.Exists) {
        return Enumerable.Empty<IModelFileBundle>();
      }

      var fileHierarchy = new FileHierarchy(midnightClub2Directory);

      return new FileHierarchyAssetBundleSeparator<IModelFileBundle>(
          fileHierarchy,
          subdir => subdir.FilesWithExtension(".xmod")
                          .Select(file => new XmodModelFileBundle {
                              GameName = "midnight_club_2",
                              XmodFile = file,
                          })
                          .Concat<IModelFileBundle>(
                              subdir.FilesWithExtension(".ped")
                                    .Select(
                                        file => new PedModelFileBundle {
                                            PedFile = file,
                                        }))).GatherFileBundles(assert);
    }
  }
}