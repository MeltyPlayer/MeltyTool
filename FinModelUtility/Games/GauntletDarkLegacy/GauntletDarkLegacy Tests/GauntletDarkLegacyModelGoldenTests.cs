using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using gdl.api;


namespace gdl;

public sealed class GauntletDarkLegacyModelGoldenTests
    : BModelGoldenTests<GauntletDarkLegacyModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override GauntletDarkLegacyModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        ObjectsFile = directory.AssertGetExistingFile("objects.ngc"),
        TexturesFile = directory.AssertGetExistingFile("textures.ngc"),
        AnimFile = directory.AssertGetExistingFile("ANIM.PS2"),
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}