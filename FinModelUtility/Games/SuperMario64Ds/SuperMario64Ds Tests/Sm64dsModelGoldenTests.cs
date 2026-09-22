using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using sm64ds.api;

namespace sm64ds;

public sealed class Sm64dsModelGoldenTests
    : BModelGoldenTests<Sm64dsModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override Sm64dsModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    return new Sm64dsModelFileBundle {
        GameName = "super_mario_64_ds",
        BmdFile = directory.FilesWithExtension(".bmd").Single(),
        BcaFiles = directory.FilesWithExtension(".bca").ToArray()
    };
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert
            .GetRootGoldensDirectory(Assembly.GetExecutingAssembly());
    return GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)
                       .ToArray();
  }
}