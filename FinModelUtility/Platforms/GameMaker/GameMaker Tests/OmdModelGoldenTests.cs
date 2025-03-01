using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using gm.api;

namespace gm;

public class OmdModelGoldenTests
    : BModelGoldenTests<OmdModelFileBundle, OmdModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => this.AssertGolden(goldenDirectory);

  public override OmdModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        OmdFile = directory.GetFilesWithFileType(".omd").Single()
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}