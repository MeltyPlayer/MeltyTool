using fin.common;
using fin.io.bundles;
using fin.util.progress;

using uni.util.io;

using xmod.api;


namespace uni.games.midnight_club_2;

public sealed class MidnightClub2FileBundleGatherer : INamedFileBundleGatherer {
  public string Name => "midnight_club_2";

  public FileBundleGathererPlatform Platform
    => FileBundleGathererPlatform.DESKTOP;

  public bool IsAvailable => ExtractorUtil.HasBeenExtracted(this.Name);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join(this.Name, ExtractorUtil.EXTRACTED),
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
                    XmodFile = xmodFile.Impl,
                    TextureDirectory = textureDirectory.Impl,
                });
              }

              foreach (var pedFile in subdir.FilesWithExtension(".ped")) {
                organizer.Add(new PedModelFileBundle {
                    PedFile = pedFile.Impl,
                    ModelDirectory = modelDirectory.Impl,
                    TextureDirectory = textureDirectory.Impl,
                });
              }
            })
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }
}