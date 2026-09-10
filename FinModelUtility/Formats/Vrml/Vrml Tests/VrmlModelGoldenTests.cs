using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;
using fin.ui.rendering.gl;

using QuickFont;

using vrml.api;

namespace vrml;

public sealed class VrmlModelGoldenTests
    : BModelGoldenTests<VrmlModelFileBundle, VrmlModelImporter> {
  [OneTimeSetUp]
  public void OneTimeSetUp() {
    // Initialize plugin
    TestingGl.InitOneTimeSetup();
    FreeTypeFontUtil.InitIfNeeded();
    // Initialize shared state
    var _ = new QFontDrawing();
  }

  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory) {
    TestingGl.InitBeforeEach();
    await this.AssertGolden(goldenDirectory);
  }

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