using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using fin.io;
using fin.testing;
using fin.testing.image;
using fin.util.enums;

using NUnit.Framework;

using schema.binary;

namespace fin.image.io.dxt;

public sealed class Dxt1GoldenTests : BImageGoldenTests {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(
        goldenDirectory,
        dir => {
          var inputFile = dir.GetExistingFiles().Single();

          var parts = inputFile.NameWithoutExtension.ToString().Split('_');
          var width = int.Parse(parts[0]);
          var height = int.Parse(parts[1]);
          var endianness = Endianness.Parse(parts[2]);

          return new Dxt1ImageReader(width, height).ReadImage(
              inputFile.OpenReadAsBinary(endianness));
        });

  private static IFileHierarchyDirectory[] GetGoldenDirectories_() {
    var rootGoldenDirectory
        = GoldenAssert.GetRootGoldensDirectory(Assembly.GetExecutingAssembly());
    return GoldenAssert
           .GetGoldenDirectories(
               rootGoldenDirectory.AssertGetExistingSubdir("dxt1"))
           .ToArray();
  }
}