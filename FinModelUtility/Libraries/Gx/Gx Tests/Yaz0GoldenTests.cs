using System.Reflection;

using fin.io;
using fin.testing;

using gx.compression.yaz0;

namespace gx;

public class Yaz0GoldenTests {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory) {
    GoldenAssert.AssertGoldenFiles(
        goldenDirectory,
        (inputDirectory, targetDirectory) => {
          var yaz0File = inputDirectory.FilesWithExtension(".yaz0").Single();

          var dstFile = new FinFile(Path.Join(targetDirectory.FullPath,
                                              $"{yaz0File.NameWithoutExtension}.txt"));

          new Yaz0Dec().Run(yaz0File, dstFile, false);
        });
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(GoldenAssert.GetRootGoldensDirectory(
                                 Assembly.GetExecutingAssembly()))
       .Where(dir => dir.Name is "yaz0")
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}