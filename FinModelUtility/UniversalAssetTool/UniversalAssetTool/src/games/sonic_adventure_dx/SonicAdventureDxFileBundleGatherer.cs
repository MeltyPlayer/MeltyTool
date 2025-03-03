using fin.io;
using fin.io.bundles;
using fin.util.progress;

using sonicadventure.api;

using uni.platforms.desktop;

namespace uni.games.sonic_adventure_dx;

public class SonicAdventureDxFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!SteamUtils.TryGetGameDirectory("Sonic Adventure DX",
                                        out var sadxDir)) {
      return;
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy("sonic_adventure_dx", sadxDir);

    foreach (var (name, modelFile, modelFileKey, modelFileOffset, textureFile)
             in GetFiles_(
                 fileHierarchy.Root)) {
      organizer.Add(
          new SonicAdventureModelFileBundle(name,
                                            modelFile,
                                            modelFileKey,
                                            modelFileOffset,
                                            textureFile)
              .Annotate(modelFile));
    }
  }

  private static
      IEnumerable<(string name,
          IFileHierarchyFile modelFile,
          uint modelFileKey,
          uint modelFileOffset,
          IReadOnlyTreeFile textureFile)> GetFiles_(
          IFileHierarchyDirectory root) {
    var systemDir = root.AssertGetExistingSubdir("system");

    var chrModelsDll = systemDir.AssertGetExistingFile("CHRMODELS.DLL");
    var sonicPvm = systemDir.AssertGetExistingFile("SONIC.PVM");
    yield return ("normal", chrModelsDll, 0x10000000, 0x56AF50, sonicPvm);
  }
}