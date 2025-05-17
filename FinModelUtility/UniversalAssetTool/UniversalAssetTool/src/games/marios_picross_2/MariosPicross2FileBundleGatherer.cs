using fin.common;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using MariosPicross;

namespace uni.games.marios_picross_2;

public class MariosPicross2FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFile(
            "marios_picross_2.gb",
            out var romFile)) {
      return;
    }

    var extractedDirectory
        = ExtractorUtil.GetOrCreateExtractedDirectory(romFile);

    if (extractedDirectory.IsEmpty) {
      var puzzleDefinitionImageConverter = new PicrossDefinitionImageConverter();
      var puzzleDefinitions = new PicrossDefinitionReader().Read(romFile);
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