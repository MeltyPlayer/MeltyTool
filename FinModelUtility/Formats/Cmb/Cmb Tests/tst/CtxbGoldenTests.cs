using System.Reflection;

using cmb.schema.cmb;
using cmb.schema.ctxb;

using fin.io;
using fin.testing;
using fin.testing.model;

using schema.binary;
using schema.binary.testing;

using Version = cmb.schema.cmb.Version;

namespace cmb {
  public class CtxbGoldenTests {
    [Test]
    [TestCaseSource(nameof(GetGoldenFiles_))]
    public async Task TestExportsGoldenAsExpected(
        IReadOnlySystemFile goldenFile) {
      var goldenGameDir = goldenFile.AssertGetParent();

      CmbHeader.Version = goldenGameDir.Name switch {
          "luigis_mansion_3d"  => Version.LUIGIS_MANSION_3D,
          "majoras_mask_3d"    => Version.MAJORAS_MASK_3D,
          "ocarina_of_time_3d" => Version.OCARINA_OF_TIME_3D
      };

      var br = new SchemaBinaryReader(goldenFile.OpenRead());
      await BinarySchemaAssert.ReadsAndWritesIdentically<Ctxb>(br);
    }

    private static IReadOnlySystemFile[] GetGoldenFiles_() {
      var rootGoldenDirectory
          = GoldenAssert
            .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
            .AssertGetExistingSubdir("ctxb");
      return rootGoldenDirectory.GetExistingSubdirs()
                                .SelectMany(dir => dir.GetExistingFiles())
                                .ToArray();
    }
  }
}