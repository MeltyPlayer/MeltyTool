using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.meltyplayer;

public sealed class MeltyPlayerFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "meltyplayer";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    new FileBundleGathererAccumulatorWithInput<IFileHierarchy>(
            fileHierarchy)
        .Add(_ => {
          Earthbound3dUtil.GatherFileBundlesFromHierarchy(
              organizer,
              fileHierarchy.Root.AssertGetExistingSubdir("earthbound_3d"));
        })
        .Add(_ => {
          MiscUtil.GatherFileBundlesFromHierarchy(
              organizer,
              fileHierarchy.Root.AssertGetExistingSubdir("misc"));
        })
        .Add(_ => {
          PaperMarioDirectorsCutUtil.GatherFileBundlesFromHierarchy(
              organizer,
              fileHierarchy.Root.AssertGetExistingSubdir("paper_mario_directors_cut"));
        })
        .Add(_ => {
          PokemonGold3dUtil.GatherFileBundlesFromHierarchy(
              organizer,
              fileHierarchy.Root.AssertGetExistingSubdir("pokemon_gold_3d"));
        })
        .Add(_ => {
          VolcanicPanicUtil.GatherFileBundlesFromHierarchy(
              organizer,
              fileHierarchy.Root.AssertGetExistingSubdir("volcanic_panic"));
        })
        .GatherFileBundles(organizer, mutablePercentageProgress);
  }
}