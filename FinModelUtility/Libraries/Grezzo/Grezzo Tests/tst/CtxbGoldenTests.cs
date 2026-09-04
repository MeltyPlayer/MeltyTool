using System.Reflection;

using fin.io;
using fin.testing;

using grezzo.schema.ctxb;

using schema.binary;
using schema.binary.testing;

using Version = grezzo.schema.cmb.Version;

namespace grezzo;

public sealed class CtxbGoldenTests {
  [Test]
  [TestCaseSource(nameof(GetGoldenFiles_))]
  public async Task TestExportsGoldenAsExpected(
      IReadOnlySystemFile goldenFile) {
    var goldenGameDir = goldenFile.AssertGetParent();

    var ctxb = new Ctxb{
        Version = goldenGameDir.Name switch {
            "luigis_mansion_3d"  => Version.LUIGIS_MANSION_3D,
            "majoras_mask_3d"    => Version.MAJORAS_MASK_3D,
            "ocarina_of_time_3d" => Version.OCARINA_OF_TIME_3D
        }
    };
    var br = new SchemaBinaryReader(goldenFile.OpenRead());
    ctxb.Read(br);

    ;

    await BinarySchemaAssert.ReadsAndWritesIdentically(br,
      new Ctxb {
          Version = goldenGameDir.Name switch {
              "luigis_mansion_3d"  => Version.LUIGIS_MANSION_3D,
              "majoras_mask_3d"    => Version.MAJORAS_MASK_3D,
              "ocarina_of_time_3d" => Version.OCARINA_OF_TIME_3D
          }
      });
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