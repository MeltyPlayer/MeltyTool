using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using sysdolphin.api;

namespace sysdolphin;

public class DatModelGoldenTests
    : BModelGoldenTests<DatModelFileBundle, DatModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => this.AssertGolden(goldenDirectory);

  public override DatModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory) {
    var gameName = directory.Parent.Parent.Name;
    var datFile = directory.FilesWithExtension(".dat").Single();

    return new DatModelFileBundle {
        GameName = gameName.ToString(),
        DatFile = datFile
    };
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .Where(dir => !(dir.Name is "super_smash_bros_melee"))
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}