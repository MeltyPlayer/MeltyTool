using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using hw.api;

namespace hw;

public sealed class XtdModelGoldenTests
    : BModelGoldenTests<XtdModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override XtdModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.FilesWithExtension(".xtd").Single(),
           directory.FilesWithExtension(".xtt").Single());

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
               .AssertGetExistingSubdir("xtd"))
       .ToArray();
}