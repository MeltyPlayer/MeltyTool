using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using grezzo.api;

namespace grezzo;

public sealed class CmbModelGoldenTests
    : BModelGoldenTests<CmbModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override CmbModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    var cmbFile = directory.FilesWithExtension(".cmb").Single();
    return new CmbModelFileBundle(
        cmbFile,
        directory.FilesWithExtension(".csab").ToArray(),
        directory.FilesWithExtension(".ctxb").ToArray(),
        directory.FilesWithExtension(".shpa").ToArray());
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert
          .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
          .AssertGetExistingSubdir("cmb");
    return GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)
                       .SelectMany(dir => dir.GetExistingSubdirs())
                       .ToArray();
  }
}