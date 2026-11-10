using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.doshin_the_giant;

// https://wiki.dolphin-emu.org/index.php?title=GKDP01
[GameIDs("GKDJ01", "GKDP01")]
public sealed class DoshinTheGiantFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "doshin_the_giant";
  public override string Title => "Doshin the Giant";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) { }
}