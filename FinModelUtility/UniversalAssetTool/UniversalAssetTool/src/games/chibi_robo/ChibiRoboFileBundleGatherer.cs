using sysdolphin.api;

using fin.io.bundles;
using fin.util.progress;

using ssm.api;

using fin.io;

namespace uni.games.chibi_robo;

public sealed class ChibiRoboFileBundleGatherer : BGameCubeFileBundleGatherer {
  public override string Name => "chibi_robo";
  public override string Title => "Chibi-Robo!";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var qpBinFile = fileHierarchy.Root.AssertGetExistingFile("qp.bin");
    var qpDir = fileHierarchy.Root.Impl.GetOrCreateSubdir("qpBin");
    if (qpDir.IsEmpty) {
      new QpBinArchiveExtractor().Extract(qpBinFile, qpDir);
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    foreach (var datFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".dat")) {
      organizer.Add(new DatModelFileBundle { DatFile = datFile });
    }

    foreach (var ssmFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".ssm")) {
      organizer.Add(new SsmAudioFileBundle { SsmFile = ssmFile });
    }
  }
}