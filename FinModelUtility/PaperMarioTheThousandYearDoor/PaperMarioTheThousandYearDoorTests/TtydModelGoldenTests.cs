using System.Reflection;

using fin.io;
using fin.testing.model;

using fin.testing;

using ttyd.api;

namespace ttyd;

public class TtydModelGoldenTests
    : BModelGoldenTests<TtydModelFileBundle, TtydModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => this.AssertGolden(goldenDirectory);

  public override TtydModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        ModelFile = directory.GetExistingFiles()
                             .Single(f => !f.Name.EndsWith('-'))
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .SelectMany(dir => dir.GetExistingSubdirs())
       .ToArray();
}