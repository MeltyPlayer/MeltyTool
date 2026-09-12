using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using pm.api;

namespace pm;

public sealed class PaperMarioShapeModelGoldenTests
    : BModelGoldenTests<PaperMarioShapeModelFileBundle, PaperMarioShapeModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override PaperMarioShapeModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.GetExistingFiles().Single(f => f.Name.EndsWith("_shape")), directory);

  private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert
            .GetRootGoldensDirectory(Assembly.GetExecutingAssembly());
    return GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)
                       .ToArray();
  }
}