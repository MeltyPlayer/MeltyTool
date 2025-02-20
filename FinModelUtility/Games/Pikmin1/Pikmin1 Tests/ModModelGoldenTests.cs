using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using NUnit.Framework;

using pikmin1.api;

namespace pikmin1 {
  public class ModModelGoldenTests
      : BModelGoldenTests<ModModelFileBundle, ModModelImporter> {
    [Test]
    [TestCaseSource(nameof(GetGoldenDirectories_))]
    public void TestExportsGoldenAsExpected(
        IFileHierarchyDirectory goldenDirectory)
      => this.AssertGolden(goldenDirectory);

    public override ModModelFileBundle GetFileBundleFromDirectory(
        IFileHierarchyDirectory directory) {
      return new ModModelFileBundle {
          GameName = "pikmin_1",
          ModFile = directory.FilesWithExtension(".mod").Single(),
          AnmFile = directory.FilesWithExtension(".anm").SingleOrDefault(),
      };
    }

    private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
      var rootGoldenDirectory
          = GoldenAssert
              .GetRootGoldensDirectory(Assembly.GetExecutingAssembly());
      return GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)
                         .ToArray();
    }
  }
}