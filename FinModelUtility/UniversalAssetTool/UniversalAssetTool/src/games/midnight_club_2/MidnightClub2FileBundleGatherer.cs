using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms;
using uni.util.io;

using xmod.api;

namespace uni.games.midnight_club_2;

public class MidnightClub2FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            "midnight_club_2",
            out var midnightClub2Directory)) {
      return;
    }

    var fileHierarchy = ExtractorUtil.GetFileHierarchy("midnight_club_2",
      midnightClub2Directory);

    var textureDirectory =
        fileHierarchy.Root.AssertGetExistingSubdir("texture_x");

    new FileHierarchyAssetBundleSeparator(
            fileHierarchy,
            (subdir, organizer) => {
              foreach (var xmodFile in subdir.FilesWithExtension(".xmod")) {
                organizer.Add(new XmodModelFileBundle {
                    GameName = "midnight_club_2",
                    XmodFile = xmodFile,
                    TextureDirectory = textureDirectory,
                }.Annotate(xmodFile));
              }

              foreach (var pedFile in subdir.FilesWithExtension(".ped")) {
                organizer.Add(new PedModelFileBundle {
                    PedFile = pedFile,
                }.Annotate(pedFile));
              }
            })
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }
}