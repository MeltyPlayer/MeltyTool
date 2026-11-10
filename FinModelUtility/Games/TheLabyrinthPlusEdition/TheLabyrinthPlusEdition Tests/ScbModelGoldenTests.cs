using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using tlpe.api;


namespace tlpe;

public sealed class ScbModelGoldenTests
    : BModelGoldenTests<ScbModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override ScbModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.GetFilesWithFileType(".scb").Single(),
           directory.AssertGetExistingFile("Balls.gam"),
           directory);

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}