using fin.io;
using fin.io.bundles;
using fin.model;
using fin.util.progress;

using gm.api;

namespace uni.games.pokemon_gold_3d;

public class PokemonGold3dFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "pokemon_gold_3d";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var omdFile in fileHierarchy.Root.GetFilesWithFileType(
                 ".omd",
                 true)) {
      organizer.Add(new AnnotatedFileBundle<OmdModelFileBundle>(
                        new OmdModelFileBundle {
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