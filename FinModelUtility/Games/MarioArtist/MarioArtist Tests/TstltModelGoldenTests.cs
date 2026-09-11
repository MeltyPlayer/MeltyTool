using System.Reflection;

using fin.io;
using fin.testing;
using fin.testing.model;

using marioartist.api;

using NUnit.Framework;

namespace marioartist;

public sealed class TstltModelGoldenTests
    : BModelGoldenTests<TstltModelFileBundle, TstltModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

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