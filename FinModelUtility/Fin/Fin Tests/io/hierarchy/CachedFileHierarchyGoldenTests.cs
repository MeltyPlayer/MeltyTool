using System.IO;
using System.Linq;
using System.Reflection;

using fin.testing;

using NUnit.Framework;

namespace fin.io.hierarchy;

public class CachedFileHierarchyGoldenTests {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory) {
    GoldenAssert.AssertGoldenFiles(
        goldenDirectory,
        (inputDirectory, targetDirectory) => new CachedFileHierarchy(
            "foobar",
            inputDirectory.Impl,
            new FinFile(
                Path.Join(targetDirectory.FullPath, "hierarchy.cache")),
            true));
  }

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(GoldenAssert.GetRootGoldensDirectory(
                                 Assembly.GetExecutingAssembly()))
       .Where(dir => dir.Name is "hierarchy")
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}