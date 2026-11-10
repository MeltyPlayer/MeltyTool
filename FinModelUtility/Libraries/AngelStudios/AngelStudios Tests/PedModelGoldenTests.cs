using System.Reflection;

using xmod.api;

using fin.io;
using fin.testing.model;
using fin.testing;

namespace xmod;

public sealed class PedModelGoldenTests : BModelGoldenTests<PedModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override PedModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        PedFile = directory.FilesWithExtension(".ped").Single(),
        ModelDirectory = directory,
        TextureDirectory = directory,
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}