using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using hm64.api;

namespace hm64;

public sealed class Hm64MapModelGoldenTests
    : BModelGoldenTests<Hm64MapModelFileBundle, Hm64MapModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override Hm64MapModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.FilesWithExtension(".bin").Single());

  private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert
            .GetRootGoldensDirectory(Assembly.GetExecutingAssembly());
    return GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)
                       .ToArray();
  }
}