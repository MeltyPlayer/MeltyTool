using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using level5.api;

namespace level5;

public sealed class XcModelGoldenTests : BModelGoldenTests<XcModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override XcModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    var modelDirectory = directory.GetExistingSubdirs()
                                  .Single(d => !d.Name.Contains('_'));
    return new XcModelFileBundle {
        ModelDirectory = modelDirectory,
        AnimationDirectories = [.. directory.GetExistingSubdirs()],
    };
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
               .AssertGetExistingSubdir("xc"))
       .ToArray();
}