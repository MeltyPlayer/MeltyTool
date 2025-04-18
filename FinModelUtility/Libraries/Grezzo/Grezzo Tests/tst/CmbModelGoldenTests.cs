using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using grezzo.api;

namespace grezzo {
  public class CmbModelGoldenTests
      : BModelGoldenTests<CmbModelFileBundle, CmbModelImporter> {
    [Test]
    [TestCaseSource(nameof(GetGoldenDirectories_))]
    public void TestExportsGoldenAsExpected(
        IFileHierarchyDirectory goldenDirectory)
      => this.AssertGolden(goldenDirectory);

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
}