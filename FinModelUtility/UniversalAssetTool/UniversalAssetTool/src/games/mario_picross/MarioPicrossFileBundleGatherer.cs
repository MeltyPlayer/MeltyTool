using fin.common;
using fin.image;
using fin.image.formats;
using fin.io;
using fin.io.bundles;
using fin.math;
using fin.util.progress;

using MarioPicross;

using SixLabors.ImageSharp.PixelFormats;

namespace uni.games.mario_picross;

public class MarioPicrossFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "mario_picross.gb",
            out var romFile)) {
      return;
    }

    var extractedDirectory
        = ExtractorUtil.GetOrCreateExtractedDirectory(romFile);

    if (extractedDirectory.IsEmpty) {
      var puzzleDefinitions = new PuzzleDefinitionReader().Read(romFile);

      foreach (var puzzleDefinition in puzzleDefinitions) {
        var name = puzzleDefinition.Name;
        if (name == "") {
          continue;
        }

        var rows = puzzleDefinition.Rows;
        var width = puzzleDefinition.Width;
        var height = puzzleDefinition.Height;

        var puzzleImage = new L8Image(PixelFormat.L8, width, height);
        using var imageLock = puzzleImage.Lock();
        var pixels = imageLock.Pixels;

        for (var y = 0; y < height; y++) {
          var row = rows[y];

          for (var x = 0; x < width; x++) {
            var cell = row.GetBit(15 - x);
            pixels[y * width + x] = new L8((byte) (cell ? 0 : 255));
          }
        }

        var imageFile
            = new FinFile(Path.Join(extractedDirectory.FullPath,
                                    $"{name}.png"));
        using var imageStream = imageFile.OpenWrite();
        puzzleImage.ExportToStream(imageStream, LocalImageFormat.PNG);
      }
    }
  }
}