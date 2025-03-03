using System.Reflection;

using fin.io;
using fin.testing;

using grezzo.schema.cmb;
using grezzo.schema.shpa;

using schema.binary;

using Version = grezzo.schema.cmb.Version;

namespace grezzo {
  public class ShpaGoldenTests {
    [Test]
    [TestCaseSource(nameof(GetGoldenFiles_))]
    public async Task TestExportsGoldenAsExpected(
        IReadOnlySystemFile goldenFile) {
      var goldenGameDir = goldenFile.AssertGetParent();

      CmbHeader.Version = goldenGameDir.Name switch {
          "luigis_mansion_3d" => Version.LUIGIS_MANSION_3D,
      };

      var er = new SchemaBinaryReader(goldenFile.OpenRead());
      await SchemaTesting.ReadsAndWritesIdentically<Shpa>(
          er,
          assertExactEndPositions: false);
    }

    private static IReadOnlySystemFile[] GetGoldenFiles_() {
      var rootGoldenDirectory
          = GoldenAssert
            .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
            .AssertGetExistingSubdir("shpa");
      return rootGoldenDirectory.GetExistingSubdirs()
                                .SelectMany(dir => dir.GetExistingFiles())
                                .ToArray();
    }
  }
}