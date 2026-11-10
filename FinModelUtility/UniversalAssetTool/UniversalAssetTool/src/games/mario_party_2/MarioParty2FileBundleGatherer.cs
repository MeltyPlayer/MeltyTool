using fin.archives;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using mp64.api;

namespace uni.games.mario_party_2;

public sealed class MarioParty2FileBundleGatherer
    : BN64FileBundleGatherer {
  public override string Name => "mario_party_2";
  public override string Title => "Mario Party 2";

  protected override void ExtractFilesFromRom(
      IReadOnlyTreeFile romFile,
      ISystemDirectory extractedDir,
      ISystemDirectory prereqsDir)
    => new Mp64MainFsImporter()
        .ExtractInto(new Mp64RomFileBundle(romFile), extractedDir);

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var dir in fileHierarchy.Root.GetExistingSubdirs()) {
      var formFiles = dir.GetFilesWithFileType(".form").Select(f => f.Impl).ToArray();
      if (!formFiles.Any()) {
        continue;
      }

      var mtxnFiles = dir.GetFilesWithFileType(".mtnx").Select(f => f.Impl).ToArray();
      foreach (var formFile in formFiles) {
        organizer.Add(new FormModelFileBundle(formFile, mtxnFiles));
      }
    }
  }
}