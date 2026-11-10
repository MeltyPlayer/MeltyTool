using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using modl.api;

namespace modl;

public sealed class ModlModelGoldenTests
    : BModelGoldenTests<ModlModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override ModlModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        GameVersion = directory.Parent.Parent.Name switch {
            "battalion_wars_1" => GameVersion.BW1,
            "battalion_wars_2" => GameVersion.BW2,
        },
        ModlFile = directory.FilesWithExtension(".modl").Single(),
        AnimFiles = directory.FilesWithExtension(".anim").ToArray(),
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
               .AssertGetExistingSubdir("modl"))
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}