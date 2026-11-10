using fin.image;
using fin.io;

namespace fin.testing.image;

public abstract class BImageGoldenTests {
  public async Task AssertGolden(
      IFileHierarchyDirectory goldenDirectory,
      Func<IFileHierarchyDirectory, IReadOnlyImage> generateImage) {
    FinImage.Initialize();

    await GoldenAssert.AssertGoldenFiles(
        goldenDirectory,
        (inputDirectory, targetDirectory) => {
          var image = generateImage(inputDirectory);

          var outputFile
              = new FinFile(Path.Join(targetDirectory.FullPath, "output.png"));

          using var fs = outputFile.OpenWrite();
          image.ExportToStream(fs, LocalImageFormat.PNG);
        });
  }
}