using System.Reflection;

using fin.io;
using fin.testing.model;

using dat.api;

using fin.testing;

namespace dat;

public class MeleeModelGoldenTests
    : BModelGoldenTests<MeleeModelFileBundle, MeleeModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => this.AssertGolden(goldenDirectory);

  public override MeleeModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    var gameName = directory.Parent.Parent.Name;
    var datFiles = directory.FilesWithExtension(".dat").ToArray();
    if (datFiles.Length == 1) {
      return new MeleeModelFileBundle {
          GameName = gameName,
          PrimaryDatFile = datFiles.Single(),
      };
    }

    return new MeleeModelFileBundle {
        GameName = gameName,
        PrimaryDatFile = datFiles.Single(f => f.Name.EndsWith("Nr.dat")),
        AnimationDatFile = datFiles.Single(f => f.Name.EndsWith("AJ.dat")),
        FighterDatFile = datFiles.Single(f => !f.Name.EndsWith("Nr.dat") &&
                                              !f.Name.EndsWith("AJ.dat")),
    };
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .Where(dir => dir.Name == "super_smash_bros_melee")
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}