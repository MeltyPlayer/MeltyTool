using System.Reflection;

using fin.io;
using fin.testing.model;

using fin.testing;

using ttyd.api;

namespace ttyd;

public sealed class TtydModelGoldenTests
    : BModelGoldenTests<TtydModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

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
       .ToArray();
}