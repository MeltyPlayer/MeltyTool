using fin.io;
using fin.io.bundles;
using fin.util.progress;

using ssm.api;

namespace uni.games.custom_robo;

// https://wiki.dolphin-emu.org/index.php?title=GXCE01
[GameIDs("GXCE01", "GXCJ01")]
public sealed class CustomRoboFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "custom_robo";
  public override string Title => "Custom Robo";

  public override bool IsListed => false;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var ssmFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".ssm")) {
      organizer.Add(new SsmAudioFileBundle {
          SsmFile = ssmFile,
      });
    }
  }
}