using fin.common;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using MariosPicross;

namespace uni.games.marios_picross;

public class MariosPicrossFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "marios_picross.gb",
            out var romFile)) {
      return;
    }

    var extractedDirectory
        = ExtractorUtil.GetOrCreateExtractedDirectory(romFile);

    if (extractedDirectory.IsEmpty) {
      var puzzleDefinitionImageConverter = new PuzzleDefinitionImageConverter();
      var puzzleDefinitions = new PuzzleDefinitionReader().Read(romFile);
      foreach (var puzzleDefinition in puzzleDefinitions) {
        var name = puzzleDefinition.Name;
        if (name == "") {
          continue;
        }

        var puzzleImage
            = puzzleDefinitionImageConverter.ConvertToImage(puzzleDefinition);
        var imageFile = new FinFile(Path.Join(extractedDirectory.FullPath,
                                              $"{name}.png"));
        using var imageStream = imageFile.OpenWrite();
        puzzleImage.ExportToStream(imageStream, LocalImageFormat.PNG);
      }
    }
  }
}