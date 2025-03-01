using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;
using fin.util.enumerables;

using modl.api;

namespace modl;

public class OutModelGoldenTests
    : BModelGoldenTests<OutModelFileBundle, OutModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => this.AssertGolden(goldenDirectory);

  public override OutModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        GameVersion = directory.Parent.Parent.Name switch {
            "battalion_wars_1" => GameVersion.BW1,
            "battalion_wars_2" => GameVersion.BW2,
        },
        OutFile = directory.FilesWithExtension(".out").Single(),
        TextureDirectories = directory.Yield(),
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
               .AssertGetExistingSubdir("out"))
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}