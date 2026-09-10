using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;
using fin.ui.rendering.gl;

using marioartist.api;

using NUnit.Framework;

using GlUtil = fin.ui.rendering.gl.GlUtil;

namespace marioartist;

public sealed class TstltModelGoldenTests
    : BModelGoldenTests<TstltModelFileBundle, TstltModelImporter> {
  [OneTimeSetUp]
  public void OneTimeSetUp() => TestingGl.InitOneTimeSetup();

  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory) {
    TestingGl.InitBeforeEach();
    await this.AssertGolden(goldenDirectory);
  }

  public override TstltModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    return new TstltModelFileBundle(directory.GetExistingFiles().Single());
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert
            .GetRootGoldensDirectory(Assembly.GetExecutingAssembly());
    return GoldenAssert
           .GetGoldenDirectories(
               rootGoldenDirectory.AssertGetExistingSubdir("tstlt"))
           .ToArray();
  }
}