using System.Reflection;

using fin.io;
using fin.testing;

using gx.compression.yay0;

namespace gx;

public class Yay0GoldenTests {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory) {
    GoldenAssert.AssertGoldenFiles(
        goldenDirectory,
        (inputDirectory, targetDirectory) => {
          var yay0File = inputDirectory.FilesWithExtension(".yay0").Single();

          var dstFile = new FinFile(Path.Join(targetDirectory.FullPath,
                                              $"{yay0File.NameWithoutExtension}.txt"));

          new Yay0Dec().Run(yay0File, dstFile, false);
        });
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(GoldenAssert.GetRootGoldensDirectory(
                                 Assembly.GetExecutingAssembly()))
       .Where(dir => dir.Name is "yay0")
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}