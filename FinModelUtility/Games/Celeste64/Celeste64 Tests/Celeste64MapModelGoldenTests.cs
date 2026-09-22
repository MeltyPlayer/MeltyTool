using System.Reflection;

using Celeste64.api;

using fin.io;
using fin.testing.model;
using fin.testing;

namespace Celeste64;

public sealed class Celeste64MapModelGoldenTests
    : BModelGoldenTests<Celeste64MapModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override Celeste64MapModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        MapFile = directory.FilesWithExtension(".map").Single(),
        TextureDirectory = directory
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}