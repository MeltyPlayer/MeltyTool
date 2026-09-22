using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using rollingMadness.api;

namespace rollingMadness;

public sealed class AseMeshModelGoldenTests
    : BModelGoldenTests<AseMeshModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override AseMeshModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.FilesWithExtension(".ase.mesh").Single(), directory);

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}