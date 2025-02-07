using fin.io.bundles;
using fin.model;
using fin.util.progress;

using gm.api;

using uni.platforms;

namespace uni.games.pokemon_gold_3d;

public class PokemonGold3dFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingSubdir(
            Path.Join("pokemon_gold_3d", ExtractorUtil.PREREQS),
            out var pg3dDir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("pokemon_gold_3d", pg3dDir);

    foreach (var omdFile in fileHierarchy.Root.GetFilesWithFileType(
                 ".omd",
                 true)) {
      organizer.Add(new AnnotatedFileBundle<OmdModelFileBundle>(
                        new OmdModelFileBundle {
                            GameName = "pokemon_gold_3d",
                            OmdFile = omdFile,
                            Mutator = TweakMaterials_,
                        },
                        omdFile));
    }
  }

  private static void TweakMaterials_(IModel model) {
    foreach (var texture in model.MaterialManager.Textures) {
      texture.MinFilter = TextureMinFilter.NEAR;
      texture.MagFilter = TextureMagFilter.NEAR;
    }
  }
}