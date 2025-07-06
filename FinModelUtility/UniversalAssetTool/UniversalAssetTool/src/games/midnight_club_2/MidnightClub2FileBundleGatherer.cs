using fin.common;
using fin.io.bundles;
using fin.util.progress;

using uni.util.io;

using xmod.api;


namespace uni.games.midnight_club_2;

public class MidnightClub2FileBundleGatherer : INamedAnnotatedFileBundleGatherer {
  public string Name => "midnight_club_2";

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join("midnight_club_2", ExtractorUtil.EXTRACTED),
            out var extractedDir)) {
      return;
    }

    var fileHierarchy = ExtractorUtil.GetFileHierarchy("midnight_club_2",
      extractedDir);

    var modelDirectory =
        fileHierarchy.Root.AssertGetExistingSubdir("model");
    var textureDirectory =
        fileHierarchy.Root.AssertGetExistingSubdir("texture_x");

    new FileHierarchyAssetBundleSeparator(
            fileHierarchy,
            (subdir, organizer) => {
              foreach (var xmodFile in subdir.FilesWithExtension(".xmod")) {
                organizer.Add(new XmodModelFileBundle {
                    XmodFile = xmodFile,
                    TextureDirectory = textureDirectory,
                }.Annotate(xmodFile));
              }

              foreach (var pedFile in subdir.FilesWithExtension(".ped")) {
                organizer.Add(new PedModelFileBundle {
                    PedFile = pedFile,
                    ModelDirectory = modelDirectory,
                    TextureDirectory = textureDirectory,
                }.Annotate(pedFile));
              }
            })
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }
}