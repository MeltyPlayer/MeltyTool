using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using vrml.api;

namespace vrml;

public class VrmlModelGoldenTests
    : BModelGoldenTests<VrmlModelFileBundle, VrmlModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => this.AssertGolden(goldenDirectory);

  public override VrmlModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        WrlFile = directory.GetFilesWithFileType(".wrl").Single()
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}