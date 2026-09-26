using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using facade.api;


namespace facade;

public sealed class FacadeModelGoldenTests
    : BModelGoldenTests<FacadeRoomModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override FacadeRoomModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.GetFilesWithFileType(".dll").Single(), directory);

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}