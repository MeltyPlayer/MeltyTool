using System.Reflection;

using fin.compression;
using fin.io;
using fin.testing;

namespace Fin.Compression_Tests;

public class Lz77GoldenTests {
  [Test]
  [TestCaseSource(nameof(Get0x10GoldenDirectories_))]
  public void Test0x10(
      IFileHierarchyDirectory goldenDirectory)
    => this.AssertGolden(goldenDirectory);

  private static IFileHierarchyDirectory[] Get0x10GoldenDirectories_()
    => GoldenAssert.GetGoldenDirectories(
                       GoldenAssert.GetRootGoldensDirectory(
                                       Assembly.GetExecutingAssembly())
                                   .AssertGetExistingSubdir("Lz77/0x10"))
                   .ToArray();

  public void AssertGolden(IFileHierarchyDirectory goldenSubdir) {
    var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
    var lz10File = inputDirectory.FilesWithExtension(".lz77").Single();

    var outputDirectory = goldenSubdir.AssertGetExistingSubdir("output");
    var hasGoldenExport = !outputDirectory.IsEmpty;

    GoldenAssert.RunInTestDirectory(
        goldenSubdir,
        tmpDirectory => {
          var targetDirectory =
              hasGoldenExport ? tmpDirectory : outputDirectory.Impl;

          using var br = lz10File.OpenReadAsBinary();
          var decompressedBytes = new Lz77Decompressor().Decompress(br);

          var targetFile = new FinFile(
              Path.Combine(targetDirectory.FullPath,
                           $"{lz10File.NameWithoutExtension}.bin"));
          targetFile.WriteAllBytes(decompressedBytes);

          if (hasGoldenExport) {
            GoldenAssert.AssertFilesInDirectoriesAreIdentical(
                tmpDirectory,
                outputDirectory.Impl);
          }
        });
  }
}