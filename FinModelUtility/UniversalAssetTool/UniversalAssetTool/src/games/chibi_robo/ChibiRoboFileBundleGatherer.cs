using sysdolphin.api;

using fin.io.bundles;
using fin.util.progress;

using ssm.api;

using uni.platforms.gcn;

namespace uni.games.chibi_robo;

public class ChibiRoboFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "chibi_robo",
            out var fileHierarchy)) {
      return;
    }

    var qpBinFile = fileHierarchy.Root.AssertGetExistingFile("qp.bin");
    var qpDir = fileHierarchy.Root.Impl.GetOrCreateSubdir("qpBin");
    if (qpDir.IsEmpty) {
      new QpBinArchiveExtractor().Extract(qpBinFile, qpDir);
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    foreach (var datFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".dat")) {
      organizer.Add(new DatModelFileBundle {
          DatFile = datFile
      }.Annotate(datFile));
    }

    foreach (var ssmFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".ssm")) {
      organizer.Add(new SsmAudioFileBundle {
          SsmFile = ssmFile,
      }.Annotate(ssmFile));
    }
  }
}