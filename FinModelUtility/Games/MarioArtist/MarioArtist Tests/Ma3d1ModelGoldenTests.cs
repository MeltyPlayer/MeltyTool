using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using NUnit.Framework;

namespace marioartist;

public sealed class Ma3d1ModelGoldenTests
    : BModelGoldenTests<Ma3d1ModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override Ma3d1ModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    return new Ma3d1ModelFileBundle(directory.GetExistingFiles().Single());
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert
            .GetRootGoldensDirectory(Assembly.GetExecutingAssembly());
    return GoldenAssert
           .GetGoldenDirectories(
               rootGoldenDirectory.AssertGetExistingSubdir("ma3d1"))
           .ToArray();
  }
}