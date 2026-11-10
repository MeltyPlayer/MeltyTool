using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using dk64.api;

namespace dk64;

public sealed class Dk64MapModelGoldenTests
    : BModelGoldenTests<Dk64MapModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override Dk64MapModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.GetExistingFiles().Single(f => f.Name.StartsWith("map")),
           directory);

  private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert
            .GetRootGoldensDirectory(Assembly.GetExecutingAssembly());
    return GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)
                       .ToArray();
  }
}