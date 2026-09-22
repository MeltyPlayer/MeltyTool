using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using sysdolphin.api;

namespace sysdolphin;

public sealed class MeleeModelGoldenTests
    : BModelGoldenTests<MeleeModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override MeleeModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    var gameName = directory.Parent.Parent.Name;
    var datFiles = directory.FilesWithExtension(".dat").ToArray();
    if (datFiles.Length == 1) {
      return new MeleeModelFileBundle {
          PrimaryDatFile = datFiles.Single(),
      };
    }

    return new MeleeModelFileBundle {
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
       .Where(dir => dir.Name is "super_smash_bros_melee")
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}