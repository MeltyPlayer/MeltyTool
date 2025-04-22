using fin.io;
using fin.io.bundles;
using fin.util.progress;

using sonicadventure.api;

using uni.platforms.desktop;

namespace uni.games.sonic_adventure_dx;

using SadxFiles = (string name, IFileHierarchyFile modelFile,
    uint modelFileKey,
    uint modelFileOffset,
    IReadOnlyTreeFile textureFile);

public class SonicAdventureDxFileBundleGatherer : IAnnotatedFileBundleGatherer {
  private IFileHierarchyFile sonicExe_;
  private IFileHierarchyFile chrModelsDll_;
  private IFileHierarchyDirectory systemDir_;

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!SteamUtils.TryGetGameDirectory("Sonic Adventure DX",
                                        out var sadxDir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("sonic_adventure_dx", sadxDir);

    var root = fileHierarchy.Root;
    this.sonicExe_ = root.AssertGetExistingFile("sonic.exe");
    this.systemDir_ = root.AssertGetExistingSubdir("system");
    this.chrModelsDll_ = this.systemDir_.TryToGetExistingFile(
        "CHRMODELS_orig.dll",
        out var chrModelsDll)
        ? chrModelsDll
        : this.systemDir_.AssertGetExistingFile("CHRMODELS.dll");

    foreach (var (name, modelFile, modelFileKey, modelFileOffset, textureFile)
             in this.GetFiles_(fileHierarchy.Root)) {
      organizer.Add(
          new SonicAdventureModelFileBundle(name,
                                            modelFile,
                                            modelFileKey,
                                            modelFileOffset,
                                            textureFile)
              .Annotate(modelFile));
    }
  }

  private IEnumerable<(string name, IFileHierarchyFile modelFile,
      uint modelFileKey,
      uint modelFileOffset,
      IReadOnlyTreeFile textureFile)> GetFiles_(
      IFileHierarchyDirectory root) {
    yield return this.ChrModelsDllModel_("Sonic", 0x56AF50, "SONIC.PVM");

    yield return this.SonicExeModel_("Eggman", 0x49C830, "EGGMAN.PVM");
    yield return this.SonicExeModel_("Ring", 0x4B4834, "OBJ_REGULAR.PVM");
  }

  private SadxFiles SonicExeModel_(string name,
                                   uint offset,
                                   string textureFileName)
    => (name, this.sonicExe_, 0x00400000, offset,
        this.systemDir_.AssertGetExistingFile(textureFileName));

  private SadxFiles ChrModelsDllModel_(string name,
                                       uint offset,
                                       string textureFileName)
    => (name, this.chrModelsDll_, 0x10000000, offset,
        this.systemDir_.AssertGetExistingFile(textureFileName));
}